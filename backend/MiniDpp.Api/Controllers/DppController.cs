using Microsoft.AspNetCore.Mvc;
using MiniDpp.Api.Services;

namespace MiniDpp.Api.Controllers;

[Route("dpp")]
public class DppController(ProductQueryService products, DppDocumentService documents) : ControllerBase
{
    [HttpGet("{gtin}.jsonld")]
    public async Task<IActionResult> JsonLd(string gtin, [FromQuery] string? rol, CancellationToken cancellationToken)
    {
        var product = await products.FindAsync(gtin, cancellationToken);
        if (product is null)
        {
            return NotFound(new { message = "Product not found." });
        }

        var json = documents.BuildJsonLd(product, DppDocumentService.ShowCarbon(rol));
        return Content(json, "application/ld+json; charset=utf-8");
    }

    [HttpGet("{gtin}")]
    public async Task<IActionResult> Html(string gtin, [FromQuery] string? rol, CancellationToken cancellationToken)
    {
        var product = await products.FindAsync(gtin, cancellationToken);
        if (product is null)
        {
            return new ContentResult
            {
                StatusCode = StatusCodes.Status404NotFound,
                ContentType = "text/html; charset=utf-8",
                Content = DppDocumentService.NotFoundHtml()
            };
        }

        var html = documents.BuildHtml(product, DppDocumentService.ShowCarbon(rol));
        return Content(html, "text/html; charset=utf-8");
    }
}
