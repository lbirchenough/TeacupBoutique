using System;

namespace inventory.Entities;

public enum ReturnCondition
{
    Good,
    Damaged,
    MissingItems
}

public class BookingItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateOnly ReservationDate { get; set; }
    public ReturnCondition? ReturnCondition { get; set; }
    public string? ReturnNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReturnedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Guid BookingId { get; set; }
    public Guid InventoryItemId { get; set; }
    public Guid ProductId { get; set; }

    // Navigation properties
    public Booking? Booking { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public Product? Product { get; set; }
}
