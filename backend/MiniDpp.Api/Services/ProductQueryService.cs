using Microsoft.EntityFrameworkCore;
using MiniDpp.Api.Data;
using MiniDpp.Api.DTOs;
using MiniDpp.Api.Models;

namespace MiniDpp.Api.Services;

public class ProductQueryService(AppDbContext db)
{
    public async Task<IReadOnlyList<ProductDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var products = await Query().OrderBy(product => product.Gtin).ToListAsync(cancellationToken);
        return products.Select(ToDto).ToList();
    }

    public Task<Product?> FindAsync(string gtin, CancellationToken cancellationToken = default) =>
        Query().FirstOrDefaultAsync(product => product.Gtin == gtin, cancellationToken);

    public async Task<ProductDto?> FindDtoAsync(string gtin, CancellationToken cancellationToken = default)
    {
        var product = await FindAsync(gtin, cancellationToken);
        return product is null ? null : ToDto(product);
    }

    /// <summary>
    /// Returns the full version history (current version first), or null if the product does not exist.
    /// </summary>
    public async Task<IReadOnlyList<ProductVersionDto>?> GetHistoryAsync(string gtin, CancellationToken cancellationToken = default)
    {
        var product = await FindAsync(gtin, cancellationToken);
        if (product is null)
        {
            return null;
        }

        var previous = await db.ProductVersions
            .AsNoTracking()
            .Where(version => version.ProductGtin == gtin)
            .OrderByDescending(version => version.VersionNumber)
            .Select(version => new ProductVersionDto
            {
                Version = version.VersionNumber,
                IsCurrent = false,
                ModelName = version.ModelName,
                BatchNumber = version.BatchNumber,
                ProductionPlace = version.ProductionPlace,
                CottonPercentage = version.CottonPercentage,
                ElastanePercentage = version.ElastanePercentage,
                CarbonKgCO2e = version.CarbonKgCO2e,
                RecordedAt = version.RecordedAt
            })
            .ToListAsync(cancellationToken);

        var current = new ProductVersionDto
        {
            Version = product.Version,
            IsCurrent = true,
            ModelName = product.Batch.ProductModel.Name,
            BatchNumber = product.Batch.BatchNumber,
            ProductionPlace = product.Batch.ProductionPlace,
            CottonPercentage = product.CottonPercentage,
            ElastanePercentage = product.ElastanePercentage,
            CarbonKgCO2e = product.CarbonKgCO2e,
            RecordedAt = product.UpdatedAt
        };

        return [current, .. previous];
    }

    public async Task<bool> DeleteAsync(string gtin, CancellationToken cancellationToken = default)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Gtin == gtin, cancellationToken);
        if (product is null)
        {
            return false;
        }

        // Version history is removed with the product (cascade delete).
        db.Products.Remove(product);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private IQueryable<Product> Query() =>
        db.Products
            .AsNoTracking()
            .Include(product => product.Batch)
            .ThenInclude(batch => batch.ProductModel);

    private static ProductDto ToDto(Product product) => new()
    {
        Gtin = product.Gtin,
        ModelName = product.Batch.ProductModel.Name,
        BatchNumber = product.Batch.BatchNumber,
        ProductionPlace = product.Batch.ProductionPlace,
        CottonPercentage = product.CottonPercentage,
        ElastanePercentage = product.ElastanePercentage,
        CarbonKgCO2e = product.CarbonKgCO2e,
        Version = product.Version,
        UpdatedAt = product.UpdatedAt
    };
}
