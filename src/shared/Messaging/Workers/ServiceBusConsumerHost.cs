using Azure.Messaging.ServiceBus;
using Messaging.Interfaces;
using Messaging.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Messaging.Workers;

public class ServiceBusConsumerHost : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly IEnumerable<IMessageConsumer> _consumers;
    private readonly ILogger<ServiceBusConsumerHost> _logger;
    private readonly List<ServiceBusProcessor> _processors = new();

    public ServiceBusConsumerHost(
        ServiceBusClient client,
        IEnumerable<IMessageConsumer> consumers,
        ILogger<ServiceBusConsumerHost> logger)
    {
        _client = client;
        _consumers = consumers;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_consumers.Any())
        {
            _logger.LogInformation("ServiceBusConsumerHost: no consumers registered");
            return;
        }

        foreach (var consumer in _consumers)
        {
            var physicalTopicName = ServiceBusTopicNameResolver.ToPhysicalTopicName(consumer.RoutingKey);
            var processor = _client.CreateProcessor(physicalTopicName, consumer.QueueName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1
            });

            var captured = consumer;
            processor.ProcessMessageAsync += async args =>
            {
                try
                {
                    await captured.HandleMessageAsync(args.Message.Body.ToString(), args.CancellationToken);
                    await args.CompleteMessageAsync(args.Message, args.CancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling message on {RoutingKey}", captured.RoutingKey);
                    await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);
                }
            };
            processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception, "Service Bus processor error on {EntityPath}", args.EntityPath);
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync(stoppingToken);
            _processors.Add(processor);
            _logger.LogInformation(
                "Listening on topic '{PhysicalTopic}' subscription '{Subscription}' (routing key: {RoutingKey})",
                physicalTopicName,
                consumer.QueueName,
                consumer.RoutingKey);
        }

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var p in _processors)
        {
            await p.StopProcessingAsync(cancellationToken);
            await p.DisposeAsync();
        }
        await base.StopAsync(cancellationToken);
    }
}
