namespace inventory.Entities;

public class ReturnAssessment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingItemId { get; set; }
    public Guid SetItemId { get; set; }
    public int QuantityGood { get; set; }
    public int QuantityDamaged { get; set; }
    public int QuantityMissing { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReplacedAt { get; set; }
    public int QuantityCustomerReturned { get; set; } = 0;

    // Navigation properties
    public BookingItem? BookingItem { get; set; }
    public SetItem? SetItem { get; set; }
}
