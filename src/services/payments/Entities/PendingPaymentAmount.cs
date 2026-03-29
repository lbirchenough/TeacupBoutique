namespace payments.Entities;

public class PendingPaymentAmount
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
