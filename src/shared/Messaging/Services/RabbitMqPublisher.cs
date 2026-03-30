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
    private readonly IConnection _connection;

    public RabbitMqPublisher(IConfiguration config, ILogger<RabbitMqPublisher> logger, IHostEnvironment env)
    {
        _logger = logger;
        _serviceName = env.ApplicationName;
        var factory = new ConnectionFactory { HostName = config["RabbitMq:Host"] };
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        using var setupChannel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        setupChannel.ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Topic).GetAwaiter().GetResult();
    }

    public async Task PublishAsync(string topic, string message)
    {
        using var channel = await _connection.CreateChannelAsync();
        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(exchange: _exchangeName, routingKey: topic, body: body);
        _logger.LogDebug("[{ServiceName}] {Topic} Sent: {Message}", _serviceName, topic, message);
    }

    public void Dispose() => _connection?.Dispose();
}
