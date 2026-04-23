using System.Text;
using Messaging.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Messaging.Workers;

public class RabbitMqConsumerHost : BackgroundService
{
    private const string ExchangeName = "commerce.events";

    private readonly IConfiguration _configuration;
    private readonly IEnumerable<IMessageConsumer> _consumers;
    private readonly ILogger<RabbitMqConsumerHost> _logger;
    private IConnection? _connection;
    private readonly List<IChannel> _channels = new();

    public RabbitMqConsumerHost(
        IConfiguration configuration,
        IEnumerable<IMessageConsumer> consumers,
        ILogger<RabbitMqConsumerHost> logger)
    {
        _configuration = configuration;
        _consumers = consumers;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_consumers.Any())
        {
            _logger.LogInformation("RabbitMqConsumerHost: no consumers registered");
            return;
        }

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

        // One connection, one channel per consumer.
        // Connection = TCP socket + AMQP handshake — expensive, servers cap them.
        // Channel = virtual stream over the connection — essentially free.
        // Channels aren't thread-safe, so one per consumer keeps handlers parallel and isolates faults.
        foreach (var consumer in _consumers)
        {
            var channel = await _connection!.CreateChannelAsync(cancellationToken: stoppingToken);
            _channels.Add(channel);

            await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, cancellationToken: stoppingToken);
            await channel.QueueDeclareAsync(queue: consumer.QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            await channel.QueueBindAsync(consumer.QueueName, ExchangeName, consumer.RoutingKey, cancellationToken: stoppingToken);

            var rmqConsumer = new AsyncEventingBasicConsumer(channel);
            var captured = consumer;
            var capturedChannel = channel;
            rmqConsumer.ReceivedAsync += async (_, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                try
                {
                    await captured.HandleMessageAsync(message, stoppingToken);
                    await capturedChannel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling message on {RoutingKey}", captured.RoutingKey);
                    await capturedChannel.BasicNackAsync(ea.DeliveryTag, false, requeue: false, stoppingToken);
                }
            };

            await channel.BasicConsumeAsync(consumer.QueueName, autoAck: false, consumer: rmqConsumer, cancellationToken: stoppingToken);
            _logger.LogInformation("Listening on routing key '{RoutingKey}' (queue: {QueueName})", consumer.RoutingKey, consumer.QueueName);
        }

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        foreach (var ch in _channels) ch.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
