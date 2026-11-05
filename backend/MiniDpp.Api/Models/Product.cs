namespace MiniDpp.Api.Models;

public class Product
{
    public required string Gtin { get; set; }

    public int BatchId { get; set; }

    public Batch Batch { get; set; } = null!;

    public decimal CottonPercentage { get; set; }

    public decimal ElastanePercentage { get; set; }

    public decimal CarbonKgCO2e { get; set; }

    /// <summary>Current version number. Starts at 1 and increments on each data change.</summary>
    public int Version { get; set; } = 1;

    /// <summary>UTC time at which the current version was recorded.</summary>
    public DateTime UpdatedAt { get; set; }

    public ICollection<ProductVersion> PreviousVersions { get; set; } = new List<ProductVersion>();
}
