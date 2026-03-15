using System.Text.Json;
using payments.Interfaces;

namespace payments.Services;

public class PaymentService(IMessagePublisher _publisher)
{
    public async Task CaptureAsync(Guid orderId)
    {
        // TODO: real payment provider integration (Stripe etc.)
        Console.WriteLine($" [payments] Capturing payment for orderId: {orderId}");

        var payload = JsonSerializer.Serialize(new { OrderId = orderId });
        await _publisher.PublishAsync("payments.PaymentSucceeded", payload);
    }

    public async Task FailAsync(Guid orderId)
    {
        Console.WriteLine($" [payments] Failing payment for orderId: {orderId}");

        var payload = JsonSerializer.Serialize(new { OrderId = orderId });
        await _publisher.PublishAsync("payments.PaymentFailed", payload);
    }
}
