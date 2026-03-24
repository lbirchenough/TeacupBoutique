using orders.Interfaces;

namespace orders.Workers;

public class BookingCancelledConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory) : RabbitMqConsumerBase(configuration)
{
    protected override string QueueName => "orders.booking-cancelled";
    protected override string RoutingKey => "inventory.BookingCancelled";

    protected override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        Console.WriteLine($" [orders] BookingCancelled received: {message}");

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandleBookingCancelled(message);
    }
}
