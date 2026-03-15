namespace orders.Models;

public class OrderPlacedDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<OrderPlacedItemDto> Items { get; set; } = [];
    public DateOnly ReservationDate { get; set; }
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
}

public class OrderPlacedItemDto
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal ClaimedPricePerDay { get; set; }
}
