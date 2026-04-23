using Messaging.Interfaces;
using System.Text.Json;
using notifications.Interfaces;
using notifications.Models;

namespace notifications.Workers;

public class PaymentFailedConsumer(IServiceScopeFactory scopeFactory, ILogger<PaymentFailedConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "notifications.payment-failed";
    public string RoutingKey => "orders.PaymentFailed";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("PaymentFailed received: {Message}", message);

        var evt = JsonSerializer.Deserialize<PaymentFailedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (evt is null) return;

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendPaymentFailedAsync(evt);
    }
}
