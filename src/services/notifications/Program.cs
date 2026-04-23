using System.Net.Http.Headers;
using System.Text;
using Messaging.DependencyInjection;
using notifications.Interfaces;
using notifications.Services;
using notifications.Workers;

var builder = Host.CreateApplicationBuilder(args);
var config = builder.Configuration;

// Email — Mailgun via HttpClient with Basic auth
builder.Services.AddHttpClient<IEmailService, MailgunEmailService>(client =>
{
    client.BaseAddress = new Uri("https://api.mailgun.net/");
    var apiKey = config["Mailgun:ApiKey"]!;
    var encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{apiKey}"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encoded);
});

builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddConsumer<OrderConfirmedConsumer>();
builder.Services.AddConsumer<PaymentFailedConsumer>();
builder.Services.AddConsumer<OrderCancelledConsumer>();
builder.Services.AddConsumer<OrderCompletedConsumer>();
builder.Services.AddConsumer<EmailChangedConsumer>();
builder.Services.AddConsumer<PasswordResetConsumer>();
builder.Services.AddConsumer<EmailVerificationConsumer>();
builder.Services.AddConsumer<EmailChangeVerificationConsumer>();
builder.Services.AddConsumer<PasswordChangedConsumer>();
builder.Services.AddMessaging(builder.Configuration);

var host = builder.Build();
host.Run();
