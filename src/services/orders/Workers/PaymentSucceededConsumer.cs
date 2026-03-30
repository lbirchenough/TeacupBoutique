using Messaging.Workers;
using orders.Interfaces;

namespace orders.Workers;

public class PaymentSucceededConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory) : RabbitMqConsumerBase(configuration)
{
    public override string QueueName => "orders.payment-succeeded";
    public override string RoutingKey => "payments.PaymentSucceeded";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        Console.WriteLine($" [orders] PaymentSucceeded received: {message}");

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandlePaymentSucceeded(message);
    }
}
