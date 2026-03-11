using orders.Interfaces;

namespace orders.Workers;

public class PaymentCapturedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory) : RabbitMqConsumerBase(configuration)
{
    protected override string QueueName => "orders.payment-captured";
    protected override string RoutingKey => "payments.PaymentCaptured";

    protected override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        Console.WriteLine($" [orders] PaymentCaptured received: {message}");

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandlePaymentCaptured(message);
    }
}
