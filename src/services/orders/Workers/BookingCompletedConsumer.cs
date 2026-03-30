using Messaging.Workers;
using orders.Interfaces;

namespace orders.Workers;

public class BookingCompletedConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory) : RabbitMqConsumerBase(configuration)
{
    public override string QueueName => "orders.booking-completed";
    public override string RoutingKey => "inventory.BookingCompleted";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        Console.WriteLine($" [orders] BookingCompleted received: {message}");

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandleBookingCompleted(message);
    }
}
