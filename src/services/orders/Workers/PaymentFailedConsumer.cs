using Messaging.Workers;
using orders.Interfaces;

namespace orders.Workers;

public class PaymentFailedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
    : RabbitMqConsumerBase(configuration, loggerFactory)
{
    public override string QueueName => "orders.payment-failed";
    public override string RoutingKey => "payments.PaymentFailed";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        _logger.LogDebug("PaymentFailed received: {Message}", message);

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandlePaymentFailed(message);
    }
}
