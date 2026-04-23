using Messaging.Interfaces;
using System.Text.Json;
using inventory.Services;

namespace inventory.Workers;

public class PaymentSucceededConsumer(IServiceScopeFactory scopeFactory, ILogger<PaymentSucceededConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "inventory.payment-succeeded";
    public string RoutingKey => "payments.PaymentSucceeded";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("PaymentSucceeded received: {Message}", message);

        var payload = JsonSerializer.Deserialize<PaymentSucceededPayload>(message);
        if (payload is null) return;

        using var scope = scopeFactory.CreateScope();
        var inventoryEventService = scope.ServiceProvider.GetRequiredService<InventoryEventService>();
        await inventoryEventService.HandlePaymentSucceeded(payload.OrderId);
    }

    private record PaymentSucceededPayload(Guid OrderId, string StripePaymentIntentId);
}
