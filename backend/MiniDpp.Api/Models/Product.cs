namespace MiniDpp.Api.Models;

public class Product
{
    public required string Gtin { get; set; }

    public int BatchId { get; set; }

    public Batch Batch { get; set; } = null!;

    public decimal CottonPercentage { get; set; }

    public decimal ElastanePercentage { get; set; }

    public decimal CarbonKgCO2e { get; set; }
}
