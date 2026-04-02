namespace inventory.Entities;

public class BookingItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateOnly ReservationDate { get; set; }
    public string? ReturnNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReturnedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Guid BookingId { get; set; }
    public Guid ProductSetId { get; set; }
    public Guid ProductId { get; set; }

    // Navigation properties
    public Booking? Booking { get; set; }
    public ProductSet? ProductSet { get; set; }
    public Product? Product { get; set; }
    public List<ReturnAssessment>? ReturnAssessments { get; set; }
    public List<BookingItemComponent>? Components { get; set; }
}
