namespace payments.Entities;

public class StripeEventRecord
{
    public string StripeEventId { get; set; } = null!;
    public string EventType { get; set; } = null!;
    public DateTime ReceivedAt { get; set; }
}
