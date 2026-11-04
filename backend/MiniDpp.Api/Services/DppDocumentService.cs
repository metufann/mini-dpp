using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using MiniDpp.Api.Models;

namespace MiniDpp.Api.Services;

public class DppDocumentService(PublicUrlBuilder publicUrlBuilder)
{
    public const string AuthorizedRole = "yetkili";

    public static bool ShowCarbon(string? rol) =>
        string.Equals(rol, AuthorizedRole, StringComparison.Ordinal);

    public string BuildHtml(Product product, bool includeCarbon)
    {
        var model = product.Batch.ProductModel;
        Func<string, string> encode = HtmlEncoder.Default.Encode;
        var culture = CultureInfo.InvariantCulture;
        var qrUrl = encode(publicUrlBuilder.QrImage(product.Gtin));
        var carbonRow = includeCarbon
            ? $$"""
                  <div class="row"><dt>Carbon</dt><dd>{{encode(product.CarbonKgCO2e.ToString("0.####", culture))}} kg CO2e</dd></div>
               """
            : string.Empty;

        return $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>Digital Product Passport — {{encode(model.Name)}}</title>
              <style>
                body { font-family: system-ui, sans-serif; margin: 0; background: #f8fafc; color: #0f172a; }
                main { max-width: 40rem; margin: 2rem auto; background: #fff; padding: 1.5rem; border: 1px solid #e2e8f0; border-radius: 1rem; }
                h1 { margin-top: 0; font-size: 1.5rem; }
                dl { display: grid; gap: 0.75rem; }
                .row { display: grid; grid-template-columns: 9rem 1fr; gap: 0.5rem; }
                dt { font-weight: 600; color: #475569; }
                dd { margin: 0; }
                img { width: 10rem; height: 10rem; margin-top: 1rem; }
              </style>
            </head>
            <body>
              <main>
                <h1>Digital Product Passport</h1>
                <dl>
                  <div class="row"><dt>Model</dt><dd>{{encode(model.Name)}}</dd></div>
                  <div class="row"><dt>GTIN</dt><dd>{{encode(product.Gtin)}}</dd></div>
                  <div class="row"><dt>Batch</dt><dd>{{encode(product.Batch.BatchNumber)}}</dd></div>
                  <div class="row"><dt>Production place</dt><dd>{{encode(product.Batch.ProductionPlace)}}</dd></div>
                  <div class="row"><dt>Cotton</dt><dd>{{encode(product.CottonPercentage.ToString("0.##", culture))}}%</dd></div>
                  <div class="row"><dt>Elastane</dt><dd>{{encode(product.ElastanePercentage.ToString("0.##", culture))}}%</dd></div>
                  {{carbonRow}}
                </dl>
                <p>Scan to open this passport:</p>
                <img src="{{qrUrl}}" alt="QR code for product {{encode(product.Gtin)}}">
              </main>
            </body>
            </html>
            """;
    }

    public string BuildJsonLd(Product product, bool includeCarbon)
    {
        var additional = new JsonArray
        {
            PropertyValue("Batch number", product.Batch.BatchNumber),
            PropertyValue("Production place", product.Batch.ProductionPlace),
            PropertyValue("Cotton percentage", product.CottonPercentage, "P1"),
            PropertyValue("Elastane percentage", product.ElastanePercentage, "P1")
        };

        if (includeCarbon)
        {
            additional.Add(new JsonObject
            {
                ["@type"] = "PropertyValue",
                ["name"] = "Carbon footprint",
                ["value"] = product.CarbonKgCO2e,
                ["unitText"] = "kg CO2e"
            });
        }

        var document = new JsonObject
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Product",
            ["name"] = product.Batch.ProductModel.Name,
            ["gtin14"] = product.Gtin,
            ["material"] = new JsonArray("Cotton", "Elastane"),
            ["additionalProperty"] = additional
        };

        return document.ToJsonString();
    }

    public static string NotFoundHtml()
    {
        return """
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>Product not found</title>
            </head>
            <body>
              <h1>Product not found</h1>
              <p>No Digital Product Passport exists for this GTIN.</p>
            </body>
            </html>
            """;
    }

    private static JsonObject PropertyValue(string name, string value) => new()
    {
        ["@type"] = "PropertyValue",
        ["name"] = name,
        ["value"] = value
    };

    private static JsonObject PropertyValue(string name, decimal value, string unitCode) => new()
    {
        ["@type"] = "PropertyValue",
        ["name"] = name,
        ["value"] = value,
        ["unitCode"] = unitCode
    };
}
