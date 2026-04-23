using Messaging.Interfaces;
using orders.Interfaces;

namespace orders.Workers;

public class BookingCancelledConsumer(IServiceScopeFactory scopeFactory, ILogger<BookingCancelledConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "orders.booking-cancelled";
    public string RoutingKey => "inventory.BookingCancelled";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("BookingCancelled received: {Message}", message);

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandleBookingCancelled(message);
    }
}
