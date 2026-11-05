namespace MiniDpp.Api.DTOs;

public class ProductDto
{
    public required string Gtin { get; set; }
    public required string ModelName { get; set; }
    public required string BatchNumber { get; set; }
    public required string ProductionPlace { get; set; }
    public decimal CottonPercentage { get; set; }
    public decimal ElastanePercentage { get; set; }
    public decimal CarbonKgCO2e { get; set; }
    public int Version { get; set; }
    public DateTime UpdatedAt { get; set; }
}
