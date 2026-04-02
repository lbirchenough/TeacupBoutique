using System.ComponentModel.DataAnnotations.Schema;

namespace inventory.Entities;

public class BookingItemComponent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingItemId { get; set; }
    public Guid SetItemId { get; set; }
    public required string Name { get; set; }
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal DepositValuePerUnit { get; set; }

    public BookingItem? BookingItem { get; set; }
    public SetItem? SetItem { get; set; }
}
