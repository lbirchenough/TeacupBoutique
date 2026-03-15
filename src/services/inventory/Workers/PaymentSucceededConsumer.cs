using System.Text.Json;
using inventory.Services;

namespace inventory.Workers;

public class PaymentSucceededConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory)
    : RabbitMqConsumerBase(configuration)
{
    protected override string QueueName => "inventory.payment-succeeded";
    protected override string RoutingKey => "payments.PaymentSucceeded";

    protected override async Task HandleMessageAsync(string message, CancellationToken ct)
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
