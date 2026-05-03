using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using Messaging.Interfaces;
using Messaging.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Messaging.Services;

public class ServiceBusPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();
    private readonly ILogger<ServiceBusPublisher> _logger;
    private readonly string _serviceName;
    private readonly IReadOnlyDictionary<string, string[]> _eventTargets;

    public ServiceBusPublisher(
        ServiceBusClient client,
        ILogger<ServiceBusPublisher> logger,
        IHostEnvironment env,
        IOptions<ServiceBusOptions> options)
    {
        _client = client;
        _logger = logger;
        _serviceName = env.ApplicationName;
        _eventTargets = options.Value.EventTargets;
    }

    public async Task PublishAsync(string topic, string message)
    {
        if (!_eventTargets.TryGetValue(topic, out var targetQueues) || targetQueues.Length == 0)
        {
            // Throw loudly: a missing mapping would otherwise silently drop messages forever.
            throw new InvalidOperationException(
                $"[{_serviceName}] No ServiceBus:EventTargets mapping configured for event '{topic}'. " +
                "Add the event and its target queue(s) to appsettings.json.");
        }

        foreach (var queueName in targetQueues)
        {
            var sender = _senders.GetOrAdd(queueName, _client.CreateSender);
            var sbMessage = new ServiceBusMessage(message) { Subject = topic };
            await sender.SendMessageAsync(sbMessage);
            _logger.LogDebug("[{ServiceName}] {Topic} -> {QueueName} Sent: {Message}", _serviceName, topic, queueName, message);
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
        {
            await sender.DisposeAsync();
        }
    }
}
