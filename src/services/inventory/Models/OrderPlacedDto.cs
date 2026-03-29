namespace orders.Models;

public class OrderPlacedDto
{
    public Guid OrderId { get; set; }
    public List<OrderPlacedItemDto> Items { get; set; } = [];
    public DateOnly ReservationDate { get; set; }
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
}

public class OrderPlacedItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
