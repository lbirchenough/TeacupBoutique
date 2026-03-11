using System.Text.Json;
using payments.Interfaces;

namespace payments.Services;

public class PaymentService(IMessagePublisher _publisher)
{
    public async Task CaptureAsync(Guid orderId)
    {
        // TODO: real payment provider integration (Stripe etc.)
        // For now, always succeed and publish PaymentCaptured
        Console.WriteLine($" [payments] Capturing payment for orderId: {orderId}");

        var payload = JsonSerializer.Serialize(new { OrderId = orderId });
        await _publisher.PublishAsync("payments.PaymentCaptured", payload);
    }

    public async Task FailAsync(Guid orderId)
    {
        Console.WriteLine($" [payments] Failing payment for orderId: {orderId}");

        var payload = JsonSerializer.Serialize(new { OrderId = orderId });
        await _publisher.PublishAsync("payments.PaymentFailed", payload);
    }
}
