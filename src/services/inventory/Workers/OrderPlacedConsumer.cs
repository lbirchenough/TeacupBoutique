using System.Text;
using System.Text.Json;
using orders.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace inventory.Workers;

public class OrderPlacedConsumer : BackgroundService
{
    
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;

    public OrderPlacedConsumer(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var hostName = _configuration["RabbitMQ:HostName"] ?? "localhost";
        var factory = new ConnectionFactory { HostName = hostName };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync();
        await _channel.ExchangeDeclareAsync(exchange: "commerce.events", type: ExchangeType.Topic);

        // declare a server-named queue
        QueueDeclareOk queueDeclareResult = await _channel.QueueDeclareAsync();
        string queueName = queueDeclareResult.QueueName;
        await _channel.QueueBindAsync(queue: queueName, exchange: "commerce.events", routingKey: "commerce.events.OrderPlaced");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var order = JsonSerializer.Deserialize<OrderPlacedDto>(message);
            Console.WriteLine($" [inventory] Order Received-> Id:{order.OrderId}, Items: {string.Join(", ", order.Items?.Select(i => $"{i.ProductId}:{i.Quantity}:{i.ClaimedPricePerDay}"))}, PlacedAt: {order.PlacedAt}");
            await _channel!.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
        };

        await _channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);
        Console.WriteLine($" [inventory] Listening to topic 'commerce.events.OrderPlaced' on queue:'{queueName}'. Press Ctrl+C to exit.");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
