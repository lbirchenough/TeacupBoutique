using System.ComponentModel.DataAnnotations.Schema;

namespace orders.Entities;

public enum OrderStatus
{
    Pending,
    AwaitingPayment,
    Confirmed,
    Completed,
    Cancelled,
}

public enum PaymentStatus
{
    Pending,
    Paid,
    Refunded
}

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Customer-facing order number, e.g. "ORD-2025-0042".</summary>
    public required string OrderNumber { get; set; }

    //Nullable for guest checkout
    public Guid? UserId { get; set; }

    // Denormalized customer snapshot at order time
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public required string CustomerPhone { get; set; }

   
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Tax { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Total { get; set; }

    // Dates related to order fulfillment
    public DateOnly PickupDate { get; set; }
    public DateOnly ReturnDate { get; set; }
    public DateOnly ReservationDate { get; set; }

    // Event details
    
    // public string? EventType { get; set; }
    // public int? GuestCount { get; set; }
    // public string? SpecialRequests { get; set; }

    public PaymentStatus? PaymentStatus { get; set; }
    public Guid? PaymentId { get; set; }

    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public List<OrderItem>? OrderItems { get; set; }
}
