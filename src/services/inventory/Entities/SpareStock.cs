namespace inventory.Entities;

public class SpareStock
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SetItemId { get; set; }
    public int QuantityAvailable { get; set; }

    // Navigation properties
    public SetItem? SetItem { get; set; }
}
