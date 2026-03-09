using System;
using System.Text;
using orders.Interfaces;
using RabbitMQ.Client;

namespace orders.Services;

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly string _exchangeName = "commerce.events";
    private readonly IConnection _connection;

    public RabbitMqPublisher(IConfiguration config)
    {
        var factory = new ConnectionFactory { HostName = config["RabbitMq:Host"] };
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        // Declare exchange once at startup and then declare new channels for each publish
        using var setupChannel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        setupChannel.ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Topic).GetAwaiter().GetResult();
    }
    
    public async Task PublishAsync(string topic, string message)
    {
        
        using var channel = await _connection.CreateChannelAsync();
        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(exchange: _exchangeName, routingKey: topic, body: body);
        Console.WriteLine($" [x] Sent {message}");
    }


    public void Dispose() => _connection?.Dispose();
}
