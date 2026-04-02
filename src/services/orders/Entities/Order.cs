using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace orders.Entities;

public enum OrderStatus
{
    Pending,
    AwaitingPayment,
    Confirmed,
    Completed,
    Cancelled,
    OutOfStock,
    PendingMissingItems
}

public enum PaymentStatus
{
    Pending,
    Paid,
}

public enum RefundStatus
{
    None,
    DepositRefunded,            // deposit returned — check Order.Status for context (completed vs late cancellation)
    DepositPartiallyRefunded,   // partial deposit returned (damages kept)
    FullyRefunded,              // early cancellation — everything returned
}

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Customer-facing order number, e.g. "ORD-2025-0042".</summary>
    public required string OrderNumber { get; set; }

    //Nullable for guest checkout
    public Guid? UserId { get; set; }

    /// <summary>Secret token for guest order tracking links.</summary>
    public Guid AccessToken { get; set; } = Guid.NewGuid();

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

    [Column(TypeName = "decimal(10,2)")]
    public decimal DepositTotal { get; set; }

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

    public RefundStatus RefundStatus { get; set; } = RefundStatus.None;

    [Column(TypeName = "decimal(10,2)")]
    public decimal AmountRefunded { get; set; } = 0;

    public int PaymentAttempts { get; set; } = 0;

    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    [Column(TypeName = "decimal(10,2)")] public decimal? DepositAmountKept { get; set; }
    public string? CompletionNotes { get; set; }

    [JsonIgnore]
    public string? ReturnPhotoUrls { get; set; }

    [NotMapped]
    [JsonPropertyName("returnPhotoUrls")]
    public List<string> ReturnPhotoUrlsList =>
        string.IsNullOrEmpty(ReturnPhotoUrls) ? [] :
        JsonSerializer.Deserialize<List<string>>(ReturnPhotoUrls) ?? [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public List<OrderItem>? OrderItems { get; set; }
}
