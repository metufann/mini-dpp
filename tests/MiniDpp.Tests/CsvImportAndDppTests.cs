using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MiniDpp.Api.DTOs;
using MiniDpp.Api.Validators;
using SkiaSharp;
using ZXing;
using ZXing.Common;

namespace MiniDpp.Tests;

public class CsvImportAndDppTests
{
    [Fact]
    public async Task SampleCsv_ImportsValidRowsAndReportsInvalidRows()
    {
        await using var factory = new MiniDppApiFactory();
        using var client = factory.CreateClient();
        var samplePath = TestFiles.SampleCsvPath();
        var expected = PredictImport(samplePath);

        var result = await ImportFile(client, samplePath);

        Assert.Equal(expected.TotalRows, result.TotalRows);
        Assert.Equal(expected.SavedRows, result.SavedRows);
        Assert.Equal(expected.FailedRows, result.FailedRows);
        Assert.Equal(expected.Errors.Select(error => error.Row), result.Errors.Select(error => error.Row));

        var products = await client.GetFromJsonAsync<List<ProductDto>>("/api/products");
        Assert.NotNull(products);
        Assert.Equal(expected.SavedRows, products!.Count);
        Assert.All(products, product => Assert.StartsWith("0", product.Gtin));
    }

    [Fact]
    public async Task Import_ContinuesAfterInvalidRows_AndUsesHeaderInclusiveLineNumbers()
    {
        await using var factory = new MiniDppApiFactory();
        using var client = factory.CreateClient();

        var csv = """
            gtin,model_adi,batch_no,elyaf_pamuk_yuzde,elyaf_elastan_yuzde,uretim_yeri,karbon_kg_co2e
            08690000000012,Slim Denim 501,B2027-01,98,2,Denizli,12.4
            ,Broken Row,B2027-03,100,0,Bursa,13.1
            08690000000029,Regular Denim 502,B2027-02,99,1,Denizli,11.8
            """;

        var result = await ImportCsv(client, csv);

        Assert.Equal(3, result.TotalRows);
        Assert.Equal(2, result.SavedRows);
        Assert.Equal(1, result.FailedRows);
        var error = Assert.Single(result.Errors);
        Assert.Equal(3, error.Row);
        Assert.Contains("GTIN cannot be empty.", error.Reasons);
    }

    [Fact]
    public async Task Import_RejectsDuplicateGtinsWithoutCreatingSecondRecord()
    {
        await using var factory = new MiniDppApiFactory();
        using var client = factory.CreateClient();
        var samplePath = TestFiles.SampleCsvPath();

        var first = await ImportFile(client, samplePath);
        var second = await ImportFile(client, samplePath);

        Assert.True(first.SavedRows > 0);
        Assert.Equal(0, second.SavedRows);
        Assert.Equal(second.TotalRows, second.FailedRows);

        var duplicateRows = second.Errors.Where(error =>
            error.Reasons.Contains("A product with this GTIN already exists.")).ToList();
        Assert.Equal(first.SavedRows, duplicateRows.Count);

        var products = await client.GetFromJsonAsync<List<ProductDto>>("/api/products");
        Assert.Equal(first.SavedRows, products!.Count);
    }

    [Fact]
    public async Task DppEndpoints_ReturnHtmlJsonLdQrAndRespectCarbonVisibility()
    {
        await using var factory = new MiniDppApiFactory();
        using var client = factory.CreateClient();
        var import = await ImportFile(client, TestFiles.SampleCsvPath());
        var gtin = (await client.GetFromJsonAsync<List<ProductDto>>("/api/products"))!.First().Gtin;
        var product = await client.GetFromJsonAsync<ProductDto>($"/api/products/{gtin}");
        Assert.NotNull(product);

        var html = await client.GetAsync($"/dpp/{gtin}");
        Assert.Equal(HttpStatusCode.OK, html.StatusCode);
        Assert.Equal("text/html", html.Content.Headers.ContentType?.MediaType);
        var htmlBody = await html.Content.ReadAsStringAsync();
        Assert.Contains(product!.ModelName, htmlBody);
        Assert.Contains(gtin, htmlBody);
        Assert.DoesNotContain("Carbon", htmlBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(product.CarbonKgCO2e.ToString("0.####", CultureInfo.InvariantCulture), htmlBody);
        Assert.DoesNotContain("karbon", htmlBody, StringComparison.OrdinalIgnoreCase);

        var htmlAuthorized = await client.GetStringAsync($"/dpp/{gtin}?rol=yetkili");
        Assert.Contains("Carbon", htmlAuthorized);
        Assert.Contains(product.CarbonKgCO2e.ToString("0.####", CultureInfo.InvariantCulture), htmlAuthorized);

        var jsonLd = await client.GetAsync($"/dpp/{gtin}.jsonld");
        Assert.Equal(HttpStatusCode.OK, jsonLd.StatusCode);
        Assert.Equal("application/ld+json", jsonLd.Content.Headers.ContentType?.MediaType);
        using var publicDoc = JsonDocument.Parse(await jsonLd.Content.ReadAsStringAsync());
        Assert.Equal("https://schema.org", publicDoc.RootElement.GetProperty("@context").GetString());
        Assert.Equal("Product", publicDoc.RootElement.GetProperty("@type").GetString());
        Assert.Equal(gtin, publicDoc.RootElement.GetProperty("gtin14").GetString());
        Assert.Equal(product.ModelName, publicDoc.RootElement.GetProperty("name").GetString());
        Assert.False(JsonContainsCarbon(publicDoc.RootElement, product.CarbonKgCO2e));

        var jsonLdAuthorized = await client.GetStringAsync($"/dpp/{gtin}.jsonld?rol=yetkili");
        using var authorizedDoc = JsonDocument.Parse(jsonLdAuthorized);
        Assert.True(JsonContainsCarbon(authorizedDoc.RootElement, product.CarbonKgCO2e));

        var qr = await client.GetAsync($"/api/products/{gtin}/qr");
        Assert.Equal(HttpStatusCode.OK, qr.StatusCode);
        Assert.Equal("image/png", qr.Content.Headers.ContentType?.MediaType);
        var png = await qr.Content.ReadAsByteArrayAsync();
        Assert.Equal($"http://localhost:5141/dpp/{gtin}", DecodeQr(png));

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/dpp/00000000000000")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/dpp/00000000000000.jsonld")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/products/00000000000000")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/products/00000000000000/qr")).StatusCode);

        Assert.True(import.SavedRows > 0);
    }

    [Fact]
    public async Task Delete_RemovesProductAndReturnsNoContent()
    {
        await using var factory = new MiniDppApiFactory();
        using var client = factory.CreateClient();
        await ImportFile(client, TestFiles.SampleCsvPath());

        var products = await client.GetFromJsonAsync<List<ProductDto>>("/api/products");
        Assert.NotNull(products);
        var targetGtin = products!.First().Gtin;

        var deleteResponse = await client.DeleteAsync($"/api/products/{targetGtin}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/products/{targetGtin}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        var notFoundDelete = await client.DeleteAsync("/api/products/00000000000000");
        Assert.Equal(HttpStatusCode.NotFound, notFoundDelete.StatusCode);
    }

    [Fact]
    public async Task Import_RejectsEmptyFile()
    {
        await using var factory = new MiniDppApiFactory();
        using var client = factory.CreateClient();

        var response = await ImportCsvRaw(client, "");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static ImportResultDto PredictImport(string path)
    {
        var validator = new ProductRowValidator();
        var lines = File.ReadAllLines(path);
        var seen = new HashSet<string>();
        var predicted = new ImportResultDto();

        for (var i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }

            predicted.TotalRows++;
            var fields = SplitCsv(lines[i]);
            var row = new CsvProductRow
            {
                Gtin = fields.ElementAtOrDefault(0),
                ModelName = fields.ElementAtOrDefault(1),
                BatchNumber = fields.ElementAtOrDefault(2),
                CottonPercentage = fields.ElementAtOrDefault(3),
                ElastanePercentage = fields.ElementAtOrDefault(4),
                ProductionPlace = fields.ElementAtOrDefault(5),
                CarbonKgCO2e = fields.ElementAtOrDefault(6)
            };

            var errors = validator.Validate(row, out var validated).ToList();
            var lineNumber = i + 1;
            if (validated is null)
            {
                predicted.Errors.Add(new ImportRowErrorDto { Row = lineNumber, Reasons = errors });
                continue;
            }

            if (!seen.Add(validated.Gtin))
            {
                predicted.Errors.Add(new ImportRowErrorDto
                {
                    Row = lineNumber,
                    Reasons = ["A product with this GTIN already exists."]
                });
                continue;
            }

            predicted.SavedRows++;
        }

        predicted.FailedRows = predicted.Errors.Count;
        return predicted;
    }

    private static List<string> SplitCsv(string line) => [.. line.Split(',')];

    private static async Task<ImportResultDto> ImportFile(HttpClient client, string path)
    {
        using var content = new MultipartFormDataContent();
        var bytes = await File.ReadAllBytesAsync(path);
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        content.Add(file, "file", Path.GetFileName(path));

        var response = await client.PostAsync("/api/import", content);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ImportResultDto>();
        return result!;
    }

    private static async Task<ImportResultDto> ImportCsv(HttpClient client, string csv)
    {
        var response = await ImportCsvRaw(client, csv);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ImportResultDto>())!;
    }

    private static Task<HttpResponseMessage> ImportCsvRaw(HttpClient client, string csv)
    {
        var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(Encoding.UTF8.GetBytes(csv));
        file.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        content.Add(file, "file", "upload.csv");
        return client.PostAsync("/api/import", content);
    }

    private static bool JsonContainsCarbon(JsonElement element, decimal carbon)
    {
        var text = element.GetRawText();
        return text.Contains("Carbon footprint", StringComparison.OrdinalIgnoreCase)
            || text.Contains(carbon.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    private static string DecodeQr(byte[] png)
    {
        using var bitmap = SKBitmap.Decode(png)
            ?? throw new InvalidOperationException("QR PNG could not be decoded.");
        var pixels = new byte[bitmap.Width * bitmap.Height * 4];
        var index = 0;
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var color = bitmap.GetPixel(x, y);
                pixels[index++] = color.Red;
                pixels[index++] = color.Green;
                pixels[index++] = color.Blue;
                pixels[index++] = color.Alpha;
            }
        }

        var source = new RGBLuminanceSource(pixels, bitmap.Width, bitmap.Height, RGBLuminanceSource.BitmapFormat.RGBA32);
        var reader = new BarcodeReaderGeneric
        {
            AutoRotate = false,
            Options = new DecodingOptions
            {
                PossibleFormats = [BarcodeFormat.QR_CODE],
                TryHarder = true
            }
        };

        var result = reader.Decode(source);
        Assert.NotNull(result);
        return result!.Text;
    }
}
