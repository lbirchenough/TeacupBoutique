using System.ComponentModel.DataAnnotations.Schema;

namespace orders.Entities;

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Quantity { get; set; } = 1;
    public DateOnly ReservationDate { get; set; }
    public Guid ProductId { get; set; }

    // Denormalized snapshot at order time
    public required string Name { get; set; }
    public string? Colour { get; set; }
    public string? ImageUrl { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Total { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Adding a foreign key property for easier querying
    public Guid OrderId { get; set; }

    // Navigation properties
    public Order? Order { get; set; }      // navigation
}
