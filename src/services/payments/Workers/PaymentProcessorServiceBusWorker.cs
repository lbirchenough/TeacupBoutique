using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Messaging.Interfaces;
using Microsoft.Extensions.Logging;
using payments.Data;
using payments.Entities;
using payments.Services;
using Stripe;

namespace payments.Workers;

public class PaymentProcessorServiceBusWorker(
    ServiceBusClient client,
    IServiceScopeFactory scopeFactory,
    IMessagePublisher publisher,
    ILogger<PaymentProcessorServiceBusWorker> logger) : BackgroundService
{
    private ServiceBusProcessor? _processor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor = client.CreateProcessor(ServiceBusWebhookQueue.QueueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1
        });

        _processor.ProcessMessageAsync += async args =>
        {
            var json = args.Message.Body.ToString();
            try
            {
                var stripeEvent = EventUtility.ParseEvent(json);

                var isSucceeded = stripeEvent.Type == EventTypes.PaymentIntentSucceeded;
                var isFailed = stripeEvent.Type == EventTypes.PaymentIntentPaymentFailed;

                if ((isSucceeded || isFailed) && stripeEvent.Data.Object is PaymentIntent paymentIntent)
                {
                    paymentIntent.Metadata.TryGetValue("orderId", out var orderIdStr);

                    if (!Guid.TryParse(orderIdStr, out var orderId))
                    {
                        logger.LogWarning("No valid orderId in PaymentIntent metadata for event {EventId}", stripeEvent.Id);
                        await args.CompleteMessageAsync(args.Message, args.CancellationToken);
                        return;
                    }

                    using var scope = scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

                    var status = isSucceeded ? PaymentStatus.Succeeded : PaymentStatus.Failed;

                    var payment = new Payment
                    {
                        Id = Guid.NewGuid(),
                        OrderId = orderId,
                        StripePaymentIntentId = paymentIntent.Id,
                        StripeEventId = stripeEvent.Id,
                        Amount = paymentIntent.Amount / 100m,
                        Currency = paymentIntent.Currency,
                        Status = status,
                        CreatedAt = DateTime.UtcNow
                    };

                    db.Payments.Add(payment);
                    await db.SaveChangesAsync();

                    var routingKey = status == PaymentStatus.Succeeded
                        ? "payments.PaymentSucceeded"
                        : "payments.PaymentFailed";

                    var payload = JsonSerializer.Serialize(new { OrderId = orderId, StripePaymentIntentId = paymentIntent.Id });
                    await publisher.PublishAsync(routingKey, payload);

                    logger.LogInformation("Processed {EventType} for order {OrderId} → {RoutingKey}", stripeEvent.Type, orderId, routingKey);
                }
                else
                {
                    logger.LogDebug("Skipping unhandled event type: {EventType}", stripeEvent.Type);
                }

                await args.CompleteMessageAsync(args.Message, args.CancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing webhook");
                await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);
            }
        };

        _processor.ProcessErrorAsync += args =>
        {
            logger.LogError(args.Exception, "Service Bus processor error on {EntityPath}", args.EntityPath);
            return Task.CompletedTask;
        };

        await _processor.StartProcessingAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }
        await base.StopAsync(cancellationToken);
    }
}
