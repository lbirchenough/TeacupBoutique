using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using Messaging.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Messaging.Services;

public class ServiceBusPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();
    private readonly ILogger<ServiceBusPublisher> _logger;
    private readonly string _serviceName;

    public ServiceBusPublisher(
        ServiceBusClient client,
        ILogger<ServiceBusPublisher> logger,
        IHostEnvironment env)
    {
        _client = client;
        _logger = logger;
        _serviceName = env.ApplicationName;
    }

    public async Task PublishAsync(string topic, string message)
    {
        var physicalTopicName = ServiceBusTopicNameResolver.ToPhysicalTopicName(topic);
        var sender = _senders.GetOrAdd(physicalTopicName, _client.CreateSender);

        // Subject preserves the logical routing key for diagnostics.
        var sbMessage = new ServiceBusMessage(message) { Subject = topic };
        await sender.SendMessageAsync(sbMessage);
        _logger.LogDebug("[{ServiceName}] {Topic} ({PhysicalTopic}) Sent: {Message}", _serviceName, topic, physicalTopicName, message);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
        {
            await sender.DisposeAsync();
        }
    }
}
