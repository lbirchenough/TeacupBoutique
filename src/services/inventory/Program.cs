using inventory.Data;
using inventory.Workers;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHostedService<HelloQueueConsumer>();

// 1) EF Core + SqlServer
builder.Services.AddDbContext<InventoryDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("InventoryDb"));
});

builder.Services.AddControllers();
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
    app.MapScalarApiReference();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "inventory v1");
    });
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
