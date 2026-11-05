namespace MiniDpp.Api.DTOs;

public class ProductVersionDto
{
    public int Version { get; set; }
    public bool IsCurrent { get; set; }
    public required string ModelName { get; set; }
    public required string BatchNumber { get; set; }
    public required string ProductionPlace { get; set; }
    public decimal CottonPercentage { get; set; }
    public decimal ElastanePercentage { get; set; }
    public decimal CarbonKgCO2e { get; set; }

    /// <summary>UTC time at which this version was recorded.</summary>
    public DateTime RecordedAt { get; set; }
}

