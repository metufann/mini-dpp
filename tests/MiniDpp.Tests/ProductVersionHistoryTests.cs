using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using MiniDpp.Api.DTOs;

namespace MiniDpp.Tests;

public class ProductVersionHistoryTests
{
    private const string Header =
        "gtin,model_adi,batch_no,elyaf_pamuk_yuzde,elyaf_elastan_yuzde,uretim_yeri,karbon_kg_co2e";

    private const string Gtin = "08690000000012";

    [Fact]
    public async Task NewProduct_StartsAtVersionOne()
    {
        await using var factory = new MiniDppApiFactory();
        using var client = factory.CreateClient();

        var result = await Import(client, $"{Gtin},Slim Denim 501,B2027-01,98,2,Denizli,12.4");

        Assert.Equal(1, result.CreatedRows);
        Assert.Equal(1, result.SavedRows);

        var product = await client.GetFromJsonAsync<ProductDto>($"/api/products/{Gtin}");
        Assert.Equal(1, product!.Version);
        Assert.NotEqual(default, product.UpdatedAt);

        var history = await GetHistory(client, Gtin);
        var only = Assert.Single(history);
        Assert.Equal(1, only.Version);
        Assert.True(only.IsCurrent);
    }

    [Fact]
    public async Task ChangedRow_IncrementsVersionAndPreservesPreviousData()
    {
        await using var factory = new MiniDppApiFactory();
        using var client = factory.CreateClient();

        await Import(client, $"{Gtin},Slim Denim 501,B2027-01,98,2,Denizli,12.4");
        var v1 = await client.GetFromJsonAsync<ProductDto>($"/api/products/{Gtin}");

        var result = await Import(client, $"{Gtin},Slim Denim 501 Eco,B2028-01,95,5,Bursa,9.75");

        Assert.Equal(1, result.UpdatedRows);
        Assert.Equal(0, result.CreatedRows);
        Assert.Equal(1, result.SavedRows);

        var current = await client.GetFromJsonAsync<ProductDto>($"/api/products/{Gtin}");
        Assert.Equal(2, current!.Version);
        Assert.Equal("Slim Denim 501 Eco", current.ModelName);
        Assert.Equal(9.75m, current.CarbonKgCO2e);
        Assert.True(current.UpdatedAt >= v1!.UpdatedAt);

        var history = await GetHistory(client, Gtin);
        Assert.Equal([2, 1], history.Select(version => version.Version));
        Assert.True(history[0].IsCurrent);

        var previous = history[1];
        Assert.False(previous.IsCurrent);
        Assert.Equal("Slim Denim 501", previous.ModelName);
        Assert.Equal("B2027-01", previous.BatchNumber);
        Assert.Equal("Denizli", previous.ProductionPlace);
        Assert.Equal(98m, previous.CottonPercentage);
        Assert.Equal(2m, previous.ElastanePercentage);
        Assert.Equal(12.4m, previous.CarbonKgCO2e);
        Assert.Equal(v1.UpdatedAt, previous.RecordedAt);
    }

    [Fact]
    public async Task IdenticalRow_DoesNotCreateVersion()
    {
        await using var factory = new MiniDppApiFactory();
        using var client = factory.CreateClient();
        const string row = $"{Gtin},Slim Denim 501,B2027-01,98,2,Denizli,12.4";

        await Import(client, row);
        // Same data with different casing / number formatting must still count as identical.
        var result = await Import(client, $"{Gtin},slim denim 501,b2027-01,98.00,2.0,Denizli,12.40");

        Assert.Equal(1, result.UnchangedRows);
        Assert.Equal(0, result.SavedRows);
        Assert.Equal(1, result.FailedRows);

        var history = await GetHistory(client, Gtin);
        Assert.Single(history);
        Assert.Equal(1, history[0].Version);
    }

    [Fact]
    public async Task InvalidRow_NeverUpdatesProductOrCreatesVersion()
    {
        await using var factory = new MiniDppApiFactory();
        using var client = factory.CreateClient();

        await Import(client, $"{Gtin},Slim Denim 501,B2027-01,98,2,Denizli,12.4");
        var result = await Import(client, $"{Gtin},Slim Denim 501,B2027-01,90,5,Denizli,1.0");

        Assert.Equal(1, result.FailedRows);
        Assert.Equal(0, result.SavedRows);

        var product = await client.GetFromJsonAsync<ProductDto>($"/api/products/{Gtin}");
        Assert.Equal(1, product!.Version);
        Assert.Equal(12.4m, product.CarbonKgCO2e);
        Assert.Single(await GetHistory(client, Gtin));
    }

    [Fact]
    public async Task RepeatedUpdates_IncrementSequentially()
    {
        await using var factory = new MiniDppApiFactory();
        using var client = factory.CreateClient();

        await Import(client, $"{Gtin},Slim Denim 501,B2027-01,98,2,Denizli,12.4");
        await Import(client, $"{Gtin},Slim Denim 501,B2027-01,98,2,Denizli,11.0");
        await Import(client, $"{Gtin},Slim Denim 501,B2027-01,98,2,Denizli,11.0"); // unchanged
        await Import(client, $"{Gtin},Slim Denim 501,B2027-01,97,3,Denizli,10.5");

        var history = await GetHistory(client, Gtin);
        Assert.Equal([3, 2, 1], history.Select(version => version.Version));
        Assert.Equal([10.5m, 11.0m, 12.4m], history.Select(version => version.CarbonKgCO2e));
        Assert.True(history[0].RecordedAt >= history[1].RecordedAt);
        Assert.True(history[1].RecordedAt >= history[2].RecordedAt);
    }

    [Fact]
    public async Task DuplicateGtinInSameFile_OnlyFirstRowIsApplied()
    {
        await using var factory = new MiniDppApiFactory();
        using var client = factory.CreateClient();

        var result = await Import(client,
            $"{Gtin},Slim Denim 501,B2027-01,98,2,Denizli,12.4",
            $"{Gtin},Slim Denim 501,B2027-01,98,2,Denizli,99.9");

        Assert.Equal(1, result.CreatedRows);
        var error = Assert.Single(result.Errors);
        Assert.Equal(3, error.Row);

        var product = await client.GetFromJsonAsync<ProductDto>($"/api/products/{Gtin}");
        Assert.Equal(1, product!.Version);
        Assert.Equal(12.4m, product.CarbonKgCO2e);
    }

    [Fact]
    public async Task History_ReturnsNotFoundForUnknownGtin()
    {
        await using var factory = new MiniDppApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/products/00000000000000/versions");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeletingProduct_RemovesItsHistory()
    {
        await using var factory = new MiniDppApiFactory();
        using var client = factory.CreateClient();

        await Import(client, $"{Gtin},Slim Denim 501,B2027-01,98,2,Denizli,12.4");
        await Import(client, $"{Gtin},Slim Denim 501,B2027-01,98,2,Denizli,11.0");

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/products/{Gtin}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/products/{Gtin}/versions")).StatusCode);

        await Import(client, $"{Gtin},Slim Denim 501,B2027-01,98,2,Denizli,12.4");
        var history = await GetHistory(client, Gtin);
        Assert.Single(history);
        Assert.Equal(1, history[0].Version);
    }

    private static async Task<List<ProductVersionDto>> GetHistory(HttpClient client, string gtin)
    {
        var history = await client.GetFromJsonAsync<List<ProductVersionDto>>($"/api/products/{gtin}/versions");
        Assert.NotNull(history);
        return history!;
    }

    private static async Task<ImportResultDto> Import(HttpClient client, params string[] rows)
    {
        var csv = string.Join('\n', [Header, .. rows]);
        using var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(Encoding.UTF8.GetBytes(csv));
        file.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        content.Add(file, "file", "upload.csv");

        var response = await client.PostAsync("/api/import", content);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ImportResultDto>())!;
    }
}

