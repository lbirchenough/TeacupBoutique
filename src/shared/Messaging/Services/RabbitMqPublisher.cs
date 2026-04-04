using System.Text;
using Messaging.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Messaging.Services;

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly string _exchangeName = "commerce.events";
    private readonly string _serviceName;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;

    public RabbitMqPublisher(IConfiguration config, ILogger<RabbitMqPublisher> logger, IHostEnvironment env)
    {
        _logger = logger;
        _serviceName = env.ApplicationName;
        _factory = new ConnectionFactory { HostName = config["RabbitMq:Host"] ?? "localhost" };
    }

    private async Task EnsureConnectedAsync(CancellationToken ct = default)
    {
        if (_connection is { IsOpen: true }) return;

        while (true)
        {
            try
            {
                _connection = await _factory.CreateConnectionAsync(ct);
                using var setupChannel = await _connection.CreateChannelAsync();
                await setupChannel.ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Topic);
                _logger.LogInformation("[{ServiceName}] Connected to RabbitMQ", _serviceName);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[{ServiceName}] RabbitMQ not ready, retrying in 5s... ({Message})", _serviceName, ex.Message);
                await Task.Delay(5000, ct);
            }
        }
    }

    public async Task PublishAsync(string topic, string message)
    {
        await EnsureConnectedAsync();
        using var channel = await _connection!.CreateChannelAsync();
        var body = Encoding.UTF8.GetBytes(message);
        var props = new BasicProperties { Persistent = true };
        await channel.BasicPublishAsync(exchange: _exchangeName, routingKey: topic, mandatory: false, basicProperties: props, body: body);
        _logger.LogDebug("[{ServiceName}] {Topic} Sent: {Message}", _serviceName, topic, message);
    }

    public void Dispose() => _connection?.Dispose();
}
