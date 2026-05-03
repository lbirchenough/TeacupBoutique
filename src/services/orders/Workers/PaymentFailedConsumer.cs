using Messaging.Interfaces;
using orders.Interfaces;

namespace orders.Workers;

public class PaymentFailedConsumer(IServiceScopeFactory scopeFactory, ILogger<PaymentFailedConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "orders.payment-failed";
    public string RoutingKey => "payments.PaymentFailed";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("PaymentFailed received: {Message}", message);

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandlePaymentFailed(message);
    }
}
