namespace inventory.Entities;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public int DisplayOrder { get; set; }

    public List<Product>? Products { get; set; }
}
