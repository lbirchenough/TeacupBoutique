using System.Text;
using Messaging.Interfaces;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Messaging.Services;

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly string _exchangeName = "commerce.events";
    private readonly IConnection _connection;

    public RabbitMqPublisher(IConfiguration config)
    {
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
        Console.WriteLine($" [messaging] {topic} Sent: {message}");
    }

    public void Dispose() => _connection?.Dispose();
}
