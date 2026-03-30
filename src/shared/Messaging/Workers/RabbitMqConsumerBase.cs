using System.Text;
using Messaging.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Messaging.Workers;

public abstract class RabbitMqConsumerBase : BackgroundService, IMessageConsumer
{
    private readonly IConfiguration _configuration;
    protected readonly ILogger _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public abstract string QueueName { get; }
    public abstract string RoutingKey { get; }
    public abstract Task HandleMessageAsync(string message, CancellationToken ct);

    protected RabbitMqConsumerBase(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { HostName = _configuration["RabbitMq:Host"] ?? "localhost" };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _connection = await factory.CreateConnectionAsync(stoppingToken);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("RabbitMQ not ready, retrying in 5s... ({Message})", ex.Message);
                await Task.Delay(5000, stoppingToken);
            }
        }
        if (stoppingToken.IsCancellationRequested) return;

        _channel = await _connection!.CreateChannelAsync();
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
        _logger.LogInformation("Listening on routing key '{RoutingKey}'", RoutingKey);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
