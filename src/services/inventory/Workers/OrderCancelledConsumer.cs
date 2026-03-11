using System.Text;
using System.Text.Json;
using inventory.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace inventory.Workers;

public class OrderCancelledConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory) : BackgroundService
{
    private IConnection? _connection;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
        var factory = new ConnectionFactory { HostName = hostName };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync();
        await _channel.ExchangeDeclareAsync(exchange: "commerce.events", type: ExchangeType.Topic);

        await _channel.QueueDeclareAsync(queue: "inventory.order-cancelled", durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync(queue: "inventory.order-cancelled", exchange: "commerce.events", routingKey: "orders.OrderCancelled");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($" [inventory] OrderCancelled received: {message}");

            var payload = JsonSerializer.Deserialize<OrderCancelledPayload>(message);
            if (payload is not null)
            {
                using var scope = scopeFactory.CreateScope();
                var inventoryEventService = scope.ServiceProvider.GetRequiredService<InventoryEventService>();
                await inventoryEventService.HandleOrderCancelled(payload.OrderId);
            }

            await _channel!.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
        };

        await _channel.BasicConsumeAsync("inventory.order-cancelled", autoAck: false, consumer: consumer);
        Console.WriteLine($" [inventory] Listening on routing key 'orders.OrderCancelled'");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }

    private record OrderCancelledPayload(Guid OrderId);
}
