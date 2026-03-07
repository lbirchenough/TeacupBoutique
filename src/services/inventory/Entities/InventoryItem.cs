using System;

namespace inventory.Entities;

public enum Condition
{
    New,
    Good,
    Fair,
    Poor
}

public enum Status
{
    Available,
    Maintenance,
    Retired
}
public class InventoryItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string SerialNumber { get; set; }
    public Condition Condition { get; set; }
    public Status Status { get; set; }
    public string? ConditionNotes { get; set; }
    public string? MaintenanceHistory { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    //Adding a foreign key property for ProductId to avoid loading the full product entity when we just want to filter on product id in our queries
    public Guid ProductId { get; set; }


    // Navigation properties
    public required Product Product { get; set; }

    public List<BookingItem>? BookingItems { get; set; }
}
