namespace MiniDpp.Api.Models;

/// <summary>
/// Immutable snapshot of a product's data as it was before an update.
/// Values are denormalized so the snapshot stays accurate even if the
/// model or batch records change later.
/// </summary>
public class ProductVersion
{
    public int Id { get; set; }

    public required string ProductGtin { get; set; }

    public Product Product { get; set; } = null!;

    public int VersionNumber { get; set; }

    public required string ModelName { get; set; }

    public required string BatchNumber { get; set; }

    public required string ProductionPlace { get; set; }

    public decimal CottonPercentage { get; set; }

    public decimal ElastanePercentage { get; set; }

    public decimal CarbonKgCO2e { get; set; }

    /// <summary>UTC time at which this version was originally recorded.</summary>
    public DateTime RecordedAt { get; set; }

    public static ProductVersion SnapshotOf(Product product) => new()
    {
        ProductGtin = product.Gtin,
        VersionNumber = product.Version,
        ModelName = product.Batch.ProductModel.Name,
        BatchNumber = product.Batch.BatchNumber,
        ProductionPlace = product.Batch.ProductionPlace,
        CottonPercentage = product.CottonPercentage,
        ElastanePercentage = product.ElastanePercentage,
        CarbonKgCO2e = product.CarbonKgCO2e,
        RecordedAt = product.UpdatedAt
    };
}

