namespace MiniDpp.Api.Models;

public class Batch
{
    public int Id { get; set; }

    public required string BatchNumber { get; set; }

    public required string ProductionPlace { get; set; }

    public int ProductModelId { get; set; }

    public ProductModel ProductModel { get; set; } = null!;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
