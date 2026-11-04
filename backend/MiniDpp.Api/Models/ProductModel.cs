namespace MiniDpp.Api.Models;

public class ProductModel
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public ICollection<Batch> Batches { get; set; } = new List<Batch>();
}
