namespace inventory.Entities;

public class Photo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Url { get; set; }
    public string? PublicId { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
    public Guid ProductId { get; set; }

    // Navigation properties
    public required Product Product { get; set; }
}
