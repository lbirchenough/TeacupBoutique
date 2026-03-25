using orders.Interfaces;

namespace orders.Workers;

public class BookingCompletedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory) : RabbitMqConsumerBase(configuration)
{
    protected override string QueueName => "orders.booking-completed";
    protected override string RoutingKey => "inventory.BookingCompleted";

    protected override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        Console.WriteLine($" [orders] BookingCompleted received: {message}");

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandleBookingCompleted(message);
    }
}
