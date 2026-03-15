namespace payments.Entities;

public enum PaymentStatus { Succeeded, Failed }

public class Payment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string StripePaymentIntentId { get; set; } = null!;
    public string StripeEventId { get; set; } = null!;
    public decimal? Amount { get; set; }
    public string Currency { get; set; } = "gbp";
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
