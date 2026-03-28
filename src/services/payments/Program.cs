using Microsoft.EntityFrameworkCore;
using payments.Data;
using payments.Interfaces;
using payments.Entities;
using payments.Services;
using payments.Workers;
using Stripe;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddOpenApi();

builder.Services.AddDbContext<PaymentsDbContext>(options =>
    options.UseSqlServer(config.GetConnectionString("PaymentsDb")));

StripeConfiguration.ApiKey = config["Stripe:SecretKey"];

builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
builder.Services.AddHttpClient<TurnstileService>();
builder.Services.AddSingleton<IWebhookQueue, RabbitMqWebhookQueue>();
builder.Services.AddHostedService<PaymentProcessorWorker>();


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
app.MapPost("/payments/create-intent", async (CreatePaymentIntentRequest request, TurnstileService turnstileService) =>
{
    if (!await turnstileService.VerifyAsync(request.TurnstileToken))
        return Results.BadRequest("CAPTCHA verification failed.");

    if (request.OrderId == Guid.Empty || request.Amount <= 0)
        return Results.BadRequest("Invalid orderId or amount.");

    var service = new PaymentIntentService();
    var options = new PaymentIntentCreateOptions
    {
        Amount = (long)(request.Amount * 100), // dollars → cents
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
app.MapPost("/webhooks/stripe", async (HttpContext ctx, PaymentsDbContext db, IWebhookQueue queue) =>
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
        Console.WriteLine($" [payments] Webhook signature invalid: {ex.Message}");
        return Results.BadRequest("Invalid signature");
    }

    // Dedup — if we've seen this event before, ack and move on
    if (await db.StripeEvents.AnyAsync(e => e.StripeEventId == stripeEvent.Id))
    {
        Console.WriteLine($" [payments] Duplicate event {stripeEvent.Id}, skipping");
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

record CreatePaymentIntentRequest(Guid OrderId, decimal Amount, string TurnstileToken);
