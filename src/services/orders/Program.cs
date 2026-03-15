using Microsoft.EntityFrameworkCore;
using orders.Data;
using orders.Interfaces;
using orders.Services;
using orders.Workers;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHostedService<StockReservedConsumer>();
builder.Services.AddHostedService<StockUnavailableConsumer>();
builder.Services.AddHostedService<PaymentSucceededConsumer>();
builder.Services.AddHostedService<PaymentFailedConsumer>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
builder.Services.AddScoped<IOrderEvent, OrderEventService>();

// 1) EF Core + SqlServer
builder.Services.AddDbContext<OrdersDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("OrdersDb"));
});


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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



var app = builder.Build();

app.UseCors("SpaDev");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
