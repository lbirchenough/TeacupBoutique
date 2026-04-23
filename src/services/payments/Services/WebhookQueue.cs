using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace payments.Services;

public interface IWebhookQueue
{
    Task EnqueueAsync(string stripeEventJson);
}

public static class WebhookQueueNames
{
    // RabbitMQ uses dots; Service Bus also permits dots, so we keep one name.
    public const string Default = "payments.process-webhook";
}

public class RabbitMqWebhookQueue : IWebhookQueue, IDisposable
{
    public const string QueueName = WebhookQueueNames.Default;
    private readonly ConnectionFactory _factory;
    private readonly ILogger<RabbitMqWebhookQueue> _logger;
    private IConnection? _connection;

    public RabbitMqWebhookQueue(IConfiguration config, ILogger<RabbitMqWebhookQueue> logger)
    {
        _logger = logger;
        _factory = new ConnectionFactory { HostName = config["RabbitMq:Host"] ?? "localhost" };
    }

    private async Task EnsureConnectedAsync()
    {
        if (_connection is { IsOpen: true }) return;

        while (true)
        {
            try
            {
                _connection = await _factory.CreateConnectionAsync();
                using var ch = await _connection.CreateChannelAsync();
                await ch.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false);
                _logger.LogInformation("WebhookQueue connected to RabbitMQ");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("RabbitMQ not ready, retrying in 5s... ({Message})", ex.Message);
                await Task.Delay(5000);
            }
        }
    }

    public async Task EnqueueAsync(string stripeEventJson)
    {
        await EnsureConnectedAsync();
        using var channel = await _connection!.CreateChannelAsync();
        var body = Encoding.UTF8.GetBytes(stripeEventJson);
        var props = new BasicProperties { Persistent = true };
        await channel.BasicPublishAsync(exchange: "", routingKey: QueueName, mandatory: false, basicProperties: props, body: body);
        _logger.LogDebug("Enqueued webhook event to {QueueName}", QueueName);
    }

    public void Dispose() => _connection?.Dispose();
}

public class ServiceBusWebhookQueue : IWebhookQueue, IAsyncDisposable
{
    public const string QueueName = WebhookQueueNames.Default;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusWebhookQueue> _logger;

    public ServiceBusWebhookQueue(ServiceBusClient client, ILogger<ServiceBusWebhookQueue> logger)
    {
        _logger = logger;
        _sender = client.CreateSender(QueueName);
    }

    public async Task EnqueueAsync(string stripeEventJson)
    {
        var message = new ServiceBusMessage(stripeEventJson);
        await _sender.SendMessageAsync(message);
        _logger.LogDebug("Enqueued webhook event to {QueueName}", QueueName);
    }

    public ValueTask DisposeAsync() => _sender.DisposeAsync();
}
