using Messaging.Interfaces;
using orders.Interfaces;

namespace orders.Workers;

public class BookingCompletedConsumer(IServiceScopeFactory scopeFactory, ILogger<BookingCompletedConsumer> logger)
    : IMessageConsumer
{
    public string QueueName => "orders.booking-completed";
    public string RoutingKey => "inventory.BookingCompleted";

    public async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        logger.LogDebug("BookingCompleted received: {Message}", message);

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandleBookingCompleted(message);
    }
}
