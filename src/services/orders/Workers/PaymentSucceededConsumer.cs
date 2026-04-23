using Messaging.Interfaces;
using orders.Interfaces;

namespace orders.Workers;

public class PaymentSucceededConsumer(IServiceScopeFactory scopeFactory, ILogger<PaymentSucceededConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "orders.payment-succeeded";
    public string RoutingKey => "payments.PaymentSucceeded";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("PaymentSucceeded received: {Message}", message);

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandlePaymentSucceeded(message);
    }
}
