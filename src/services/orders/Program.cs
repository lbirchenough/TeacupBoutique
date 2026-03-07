using Microsoft.EntityFrameworkCore;
using orders.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// 1) EF Core + SqlServer
builder.Services.AddDbContext<OrdersDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("OrdersDb"));
});


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
