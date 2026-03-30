using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace payments.Services;

public interface IWebhookQueue
{
    Task EnqueueAsync(string stripeEventJson);
}

public class RabbitMqWebhookQueue : IWebhookQueue, IDisposable
{
    public const string QueueName = "payments.process-webhook";
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqWebhookQueue> _logger;

    public RabbitMqWebhookQueue(IConfiguration config, ILogger<RabbitMqWebhookQueue> logger)
    {
        _logger = logger;
        var factory = new ConnectionFactory { HostName = config["RabbitMq:Host"] };
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        using var ch = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        ch.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false).GetAwaiter().GetResult();
    }

    public async Task EnqueueAsync(string stripeEventJson)
    {
        using var channel = await _connection.CreateChannelAsync();
        var body = Encoding.UTF8.GetBytes(stripeEventJson);
        var props = new BasicProperties { Persistent = true };
        await channel.BasicPublishAsync(exchange: "", routingKey: QueueName, mandatory: false, basicProperties: props, body: body);
        _logger.LogDebug("Enqueued webhook event to {QueueName}", QueueName);
    }

    public void Dispose() => _connection?.Dispose();
}
