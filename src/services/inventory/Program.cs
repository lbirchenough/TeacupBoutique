using inventory.Data;
using inventory.Services;
using inventory.Workers;
using Messaging.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddConsumer<OrderPlacedConsumer>();
builder.Services.AddConsumer<OrderCancelledConsumer>();
builder.Services.AddConsumer<PaymentSucceededConsumer>();
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddScoped<InventoryEventService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// 1) EF Core + SqlServer
builder.Services.AddDbContext<InventoryDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("InventoryDbPostgres"));
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "inventory v1");
    });
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Cheap wake-up endpoint for gateway /api/wake. Returns immediately; the act of
// receiving the request is what triggers Container Apps to scale 0 -> 1.
app.MapGet("/health", () => Results.Ok());

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    for (var i = 0; i < 10; i++)
    {
        try { await db.Database.MigrateAsync(); break; }
        catch { await Task.Delay(3000); }
    }
}

app.Run();
