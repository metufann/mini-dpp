using Microsoft.Extensions.Options;
using MiniDpp.Api.Options;

namespace MiniDpp.Api.Services;

public class PublicUrlBuilder(IOptions<PublicAppOptions> options)
{
    private readonly string _baseUrl = options.Value.BaseUrl.TrimEnd('/');

    public string HtmlDpp(string gtin) => $"{_baseUrl}/dpp/{Uri.EscapeDataString(gtin)}";

    public string JsonLdDpp(string gtin) => $"{_baseUrl}/dpp/{Uri.EscapeDataString(gtin)}.jsonld";

    public string QrImage(string gtin) => $"{_baseUrl}/api/products/{Uri.EscapeDataString(gtin)}/qr";
}
