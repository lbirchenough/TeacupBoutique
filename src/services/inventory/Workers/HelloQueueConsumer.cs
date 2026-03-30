using Messaging.Workers;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace inventory.Workers;

public class HelloQueueConsumer : BackgroundService
{
    private const string QueueName = "hello";
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;

    public HelloQueueConsumer(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var hostName = _configuration["RabbitMq:Host"] ?? "localhost";
        var factory = new ConnectionFactory { HostName = hostName };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync();
        await _channel.QueueDeclareAsync(QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($" [inventory] Received: {message}");
            await _channel!.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
        };

        await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer, stoppingToken);
        Console.WriteLine($" [inventory] Listening on queue '{QueueName}'. Press Ctrl+C to exit.");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
