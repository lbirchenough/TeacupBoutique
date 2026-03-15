using System.Net.Http.Headers;
using System.Text;
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

builder.Services.AddHostedService<OrderConfirmedConsumer>();
builder.Services.AddHostedService<PaymentFailedConsumer>();
builder.Services.AddHostedService<OrderCancelledConsumer>();

var host = builder.Build();
host.Run();
