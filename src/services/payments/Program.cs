using payments.Interfaces;
using payments.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("SpaDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowCredentials()
            .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
builder.Services.AddScoped<PaymentService>();

var app = builder.Build();

app.UseCors("SpaDev");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/payments/{orderId}/capture", async (Guid orderId, PaymentService payments) =>
{
    await payments.CaptureAsync(orderId);
    return Results.Ok();
});

// Dev-only endpoint to simulate a payment failure
app.MapPost("/payments/{orderId}/fail", async (Guid orderId, PaymentService payments) =>
{
    await payments.FailAsync(orderId);
    return Results.Problem(detail: "Payment declined.", statusCode: 402);
})
.ExcludeFromDescription(); // hides it from OpenAPI docs

app.Run();
