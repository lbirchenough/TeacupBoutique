namespace inventory.Entities;

public enum Status
{
    Available,
    Maintenance,
    Retired
}

public class ProductSet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public Status Status { get; set; } = Status.Available;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid ProductId { get; set; }

    // Navigation properties
    public required Product Product { get; set; }
    public List<BookingItem>? BookingItems { get; set; }
}
