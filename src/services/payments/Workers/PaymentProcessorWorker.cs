using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using payments.Data;
using Messaging.Interfaces;
using payments.Entities;
using payments.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Stripe;

namespace payments.Workers;

public class PaymentProcessorWorker(
    IConfiguration config,
    IServiceScopeFactory scopeFactory,
    IMessagePublisher publisher,
    ILogger<PaymentProcessorWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { HostName = config["RabbitMq:Host"] };

        IConnection? connection = null;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                connection = await factory.CreateConnectionAsync(stoppingToken);
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning("RabbitMQ not ready, retrying in 5s... ({Message})", ex.Message);
                await Task.Delay(5000, stoppingToken);
            }
        }
        if (connection is null || stoppingToken.IsCancellationRequested) return;
        using var _ = connection;

        using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            RabbitMqWebhookQueue.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());

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
                        await channel.BasicAckAsync(ea.DeliveryTag, false);
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
                        Amount = paymentIntent.Amount / 100m, // Stripe sends pence
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

                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing webhook");
                await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
            }
        };

        await channel.BasicConsumeAsync(RabbitMqWebhookQueue.QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
