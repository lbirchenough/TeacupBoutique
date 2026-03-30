using Messaging.Workers;
using orders.Interfaces;

namespace orders.Workers;

public class BookingCancelledConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
    : RabbitMqConsumerBase(configuration, loggerFactory)
{
    public override string QueueName => "orders.booking-cancelled";
    public override string RoutingKey => "inventory.BookingCancelled";

    public override async Task HandleMessageAsync(string message, CancellationToken ct)
    {
        _logger.LogDebug("BookingCancelled received: {Message}", message);

        using var scope = scopeFactory.CreateScope();
        var orderEventService = scope.ServiceProvider.GetRequiredService<IOrderEvent>();
        await orderEventService.HandleBookingCancelled(message);
    }
}
