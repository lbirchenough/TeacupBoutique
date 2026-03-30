using Messaging.Workers;
using System.Text.Json;
using inventory.Services;

namespace inventory.Workers;

public class PaymentSucceededConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory)
    : RabbitMqConsumerBase(configuration)
{
    public override string QueueName => "inventory.payment-succeeded";
    public override string RoutingKey => "payments.PaymentSucceeded";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        Console.WriteLine($" [inventory] PaymentSucceeded received: {message}");

        var payload = JsonSerializer.Deserialize<PaymentSucceededPayload>(message);
        if (payload is null) return;

        using var scope = scopeFactory.CreateScope();
        var inventoryEventService = scope.ServiceProvider.GetRequiredService<InventoryEventService>();
        await inventoryEventService.HandlePaymentSucceeded(payload.OrderId);
    }

    private record PaymentSucceededPayload(Guid OrderId, string StripePaymentIntentId);
}
