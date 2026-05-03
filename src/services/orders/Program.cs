using Microsoft.EntityFrameworkCore;
using orders.Data;
using orders.Interfaces;
using orders.Services;
using orders.Workers;
using Messaging.DependencyInjection;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddConsumer<StockReservedConsumer>();
builder.Services.AddConsumer<StockUnavailableConsumer>();
builder.Services.AddConsumer<PaymentSucceededConsumer>();
builder.Services.AddConsumer<PaymentFailedConsumer>();
builder.Services.AddConsumer<BookingCancelledConsumer>();
builder.Services.AddConsumer<BookingCompletedConsumer>();
builder.Services.AddConsumer<RefundSucceededConsumer>();
builder.Services.AddConsumer<RefundFailedConsumer>();
builder.Services.AddConsumer<ReturnAssessedWithMissingItemsConsumer>();
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddScoped<IOrderEvent, OrderEventService>();
builder.Services.AddHttpClient<TurnstileService>();

// 1) EF Core + SqlServer
builder.Services.AddDbContext<OrdersDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("OrdersDbPostgres"));
});


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.MapControllers();

// Cheap wake-up endpoint for gateway /api/wake. Returns immediately; the act of
// receiving the request is what triggers Container Apps to scale 0 -> 1.
app.MapGet("/health", () => Results.Ok());

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    for (var i = 0; i < 10; i++)
    {
        try { await db.Database.MigrateAsync(); break; }
        catch { await Task.Delay(3000); }
    }
}

app.Run();
