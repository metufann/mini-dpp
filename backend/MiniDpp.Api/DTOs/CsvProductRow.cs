namespace MiniDpp.Api.DTOs;

public class CsvProductRow
{
    public string? Gtin { get; set; }
    public string? ModelName { get; set; }
    public string? BatchNumber { get; set; }
    public string? CottonPercentage { get; set; }
    public string? ElastanePercentage { get; set; }
    public string? ProductionPlace { get; set; }
    public string? CarbonKgCO2e { get; set; }
}
