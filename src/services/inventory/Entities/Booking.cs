using System;
using System.Data;

namespace inventory.Entities;

public enum BookingStatus
{
    Reserved,
    Confirmed,
    CheckedOut,
    Returned,
    Completed,
    Cancelled
}
public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int OrderId { get; set; }   
    public DateTime RentalDate { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
    public DateTime? ReturnedAt { get; set; }

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }


    // Navigation properties
    public List<BookingItem>? BookingItems { get; set; }

}
