using Microsoft.EntityFrameworkCore;
using payments.Data;
using payments.Entities;
using payments.Services;
using Messaging.Interfaces;
using Messaging.Services;
using payments.Workers;
using Stripe;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddOpenApi();

builder.Services.AddDbContext<PaymentsDbContext>(options =>
    options.UseNpgsql(config.GetConnectionString("PaymentsDbPostgres")));

StripeConfiguration.ApiKey = config["Stripe:SecretKey"];

builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
builder.Services.AddHttpClient<TurnstileService>();
builder.Services.AddSingleton<IWebhookQueue, RabbitMqWebhookQueue>();
builder.Services.AddHostedService<PaymentProcessorWorker>();
builder.Services.AddHostedService<ReadyForPaymentConsumer>();
builder.Services.AddHostedService<RefundRequestedConsumer>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    for (var i = 0; i < 10; i++)
    {
        try { await db.Database.MigrateAsync(); break; }
        catch { await Task.Delay(3000); }
    }
}

// --- Create PaymentIntent ---
app.MapPost("/payments/create-intent", async (CreatePaymentIntentRequest request, TurnstileService turnstileService, PaymentsDbContext db) =>
{
    if (!await turnstileService.VerifyAsync(request.TurnstileToken))
        return Results.BadRequest("CAPTCHA verification failed.");

    if (request.OrderId == Guid.Empty)
        return Results.BadRequest("Invalid orderId.");

    var pending = await db.PendingPaymentAmounts.FindAsync(request.OrderId);
    if (pending is null)
        return Results.NotFound("Order is not ready for payment yet.");

    var service = new PaymentIntentService();
    var options = new PaymentIntentCreateOptions
    {
        Amount = (long)(pending.Amount * 100), // dollars → cents
        Currency = "aud",
        PaymentMethodTypes = ["card"],
        Metadata = new Dictionary<string, string>
        {
            { "orderId", request.OrderId.ToString() }
        }
    };

    var intent = await service.CreateAsync(options);

    return Results.Ok(new { clientSecret = intent.ClientSecret });
});


// --- Stripe webhook ---
app.MapPost("/webhooks/stripe", async (HttpContext ctx, PaymentsDbContext db, IWebhookQueue queue, ILogger<Program> logger) =>
{
    string json;
    using (var reader = new StreamReader(ctx.Request.Body))
        json = await reader.ReadToEndAsync();

    var webhookSecret = config["Stripe:WebhookSecret"]!;

    Stripe.Event stripeEvent;
    try
    {
        stripeEvent = EventUtility.ConstructEvent(
            json,
            ctx.Request.Headers["Stripe-Signature"],
            webhookSecret);
    }
    catch (StripeException ex)
    {
        logger.LogWarning("Webhook signature invalid: {Message}", ex.Message);
        return Results.BadRequest("Invalid signature");
    }

    // Dedup — if we've seen this event before, ack and move on
    if (await db.StripeEvents.AnyAsync(e => e.StripeEventId == stripeEvent.Id))
    {
        logger.LogInformation("Duplicate event {EventId}, skipping", stripeEvent.Id);
        return Results.Ok();
    }

    db.StripeEvents.Add(new StripeEventRecord
    {
        StripeEventId = stripeEvent.Id,
        EventType = stripeEvent.Type,
        ReceivedAt = DateTime.UtcNow
    });
    await db.SaveChangesAsync();

    await queue.EnqueueAsync(json);

    return Results.Ok();
});


app.Run();

record CreatePaymentIntentRequest(Guid OrderId, string TurnstileToken);
