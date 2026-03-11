using orders.Interfaces;

namespace orders.Workers;

public class PaymentFailedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory) : RabbitMqConsumerBase(configuration)
{
    protected override string QueueName => "orders.payment-failed";
    protected override string RoutingKey => "payments.PaymentFailed";

    protected override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        Console.WriteLine($" [orders] PaymentFailed received: {message}");

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandlePaymentFailed(message);
    }
}
