namespace inventory.Entities;

public class Tag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }

    public List<Product>? Products { get; set; }
}
