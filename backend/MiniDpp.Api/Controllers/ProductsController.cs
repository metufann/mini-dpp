using Microsoft.AspNetCore.Mvc;
using MiniDpp.Api.DTOs;
using MiniDpp.Api.Services;

namespace MiniDpp.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(
    ProductQueryService products,
    QrCodeService qrCodeService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await products.ListAsync(cancellationToken));
    }

    [HttpGet("{gtin}")]
    public async Task<ActionResult<ProductDto>> Get(string gtin, CancellationToken cancellationToken)
    {
        var product = await products.FindDtoAsync(gtin, cancellationToken);
        if (product is null)
        {
            return NotFound(new { message = "Product not found." });
        }

        return Ok(product);
    }

    [HttpGet("{gtin}/versions")]
    public async Task<ActionResult<IReadOnlyList<ProductVersionDto>>> Versions(string gtin, CancellationToken cancellationToken)
    {
        var history = await products.GetHistoryAsync(gtin, cancellationToken);
        if (history is null)
        {
            return NotFound(new { message = "Product not found." });
        }

        return Ok(history);
    }

    [HttpDelete("{gtin}")]
    public async Task<IActionResult> Delete(string gtin, CancellationToken cancellationToken)
    {
        var deleted = await products.DeleteAsync(gtin, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { message = "Product not found." });
        }

        return NoContent();
    }

    [HttpGet("{gtin}/qr")]
    public async Task<IActionResult> Qr(string gtin, CancellationToken cancellationToken)
    {
        var product = await products.FindAsync(gtin, cancellationToken);
        if (product is null)
        {
            return NotFound(new { message = "Product not found." });
        }

        var png = qrCodeService.CreatePngForProduct(product.Gtin);
        return File(png, "image/png");
    }
}
