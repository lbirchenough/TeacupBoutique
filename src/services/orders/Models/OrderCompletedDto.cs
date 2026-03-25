namespace orders.Models;

public class OrderCompletedDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid AccessToken { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public DateOnly ReservationDate { get; set; }
    public decimal? DepositAmountKept { get; set; }
    public string? CompletionNotes { get; set; }
}
