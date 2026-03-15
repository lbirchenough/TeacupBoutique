using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace notifications.Workers;

public abstract class RabbitMqConsumerBase : BackgroundService
{
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;

    protected abstract string QueueName { get; }
    protected abstract string RoutingKey { get; }
    protected abstract Task HandleMessageAsync(string message, CancellationToken ct);

    protected RabbitMqConsumerBase(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { HostName = _configuration["RabbitMq:Host"] ?? "localhost" };
        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync();
        await _channel.ExchangeDeclareAsync("commerce.events", ExchangeType.Topic);

        await _channel.QueueDeclareAsync(queue: QueueName, durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync(QueueName, "commerce.events", RoutingKey);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            await HandleMessageAsync(message, stoppingToken);
            await _channel!.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
        };

        await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer);
        Console.WriteLine($" [notifications] Listening on routing key '{RoutingKey}'");
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
