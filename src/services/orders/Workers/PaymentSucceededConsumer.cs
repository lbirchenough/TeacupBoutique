using orders.Interfaces;

namespace orders.Workers;

public class PaymentSucceededConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory) : RabbitMqConsumerBase(configuration)
{
    protected override string QueueName => "orders.payment-succeeded";
    protected override string RoutingKey => "payments.PaymentSucceeded";

    protected override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        Console.WriteLine($" [orders] PaymentSucceeded received: {message}");

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandlePaymentSucceeded(message);
    }
}
