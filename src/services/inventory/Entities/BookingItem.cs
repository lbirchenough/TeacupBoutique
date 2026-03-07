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
    public ReturnCondition ReturnCondition { get; set; }
    public string? ReturnNotes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public required Booking Booking { get; set; }
    public required InventoryItem InventoryItem { get; set; }
    public required Product Product { get; set; }
}
