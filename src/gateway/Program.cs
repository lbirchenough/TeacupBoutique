using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using gateway;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Clear default restrictions so nginx container IP is trusted as a proxy
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSection["Key"]!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("SpaDev", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowCredentials()
            .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    options.AddPolicy("fixed-10-per-min", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"{httpContext.Connection.RemoteIpAddress}:{httpContext.Request.Path}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 10,
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("fixed-5-per-hour", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"{httpContext.Connection.RemoteIpAddress}:{httpContext.Request.Path}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromHours(1),
                PermitLimit = 5,
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("fixed-100-per-min", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"{httpContext.Connection.RemoteIpAddress}:{httpContext.Request.Path}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 100,
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("fixed-5-per-hour-user", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"{httpContext.User.FindFirst("sub")?.Value}:{httpContext.Request.Path}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromHours(1),
                PermitLimit = 5,
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

// Bounded HTTP client used by /api/wake to ping downstream /health endpoints.
// Per-request timeout is short enough that one cold-starting backend doesn't
// block the others past Container Apps' worst-case wake.
builder.Services.AddHttpClient("wake", c => c.Timeout = TimeSpan.FromSeconds(60));

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(context =>
    {
        if (context.Route.RouteId is "route4-mine" or "route4-claim" or "route4")
        {
            context.AddRequestTransform(transformContext =>
            {
                var user = transformContext.HttpContext.User;
                var userId = user.FindFirst("sub")?.Value;
                if (userId is not null)
                    transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Id", userId);

                if (context.Route.RouteId == "route4-claim")
                {
                    var email = user.FindFirst("email")?.Value;
                    if (email is not null)
                        transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Email", email);
                }

                return ValueTask.CompletedTask;
            });
        }
    });

var app = builder.Build();

app.UseForwardedHeaders();
app.UseCors("SpaDev");
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

// Frontend hits this on app load (and re-load on /checkout) to wake the
// scaled-to-zero backends before they're needed. Throttled so repeat calls
// inside the warm window are no-ops. Mapped before MapReverseProxy so it is
// served locally instead of being proxied.
app.MapPost("/api/wake", async (IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<Program> logger, CancellationToken ct) =>
{
    if (WakeThrottle.IsWarm())
    {
        return Results.Ok(new { status = "warm" });
    }

    var targets = new (string Service, string Url)[]
    {
        ("auth", config["ReverseProxy:Clusters:auth-cluster:Destinations:destination1:Address"] + "health"),
        ("inventory", config["ReverseProxy:Clusters:inventory-cluster:Destinations:destination1:Address"] + "health"),
        ("orders", config["ReverseProxy:Clusters:orders-cluster:Destinations:destination1:Address"] + "health"),
        ("payments", config["ReverseProxy:Clusters:payments-cluster:Destinations:destination1:Address"] + "health"),
    };

    var client = httpClientFactory.CreateClient("wake");
    using var overall = CancellationTokenSource.CreateLinkedTokenSource(ct);
    overall.CancelAfter(TimeSpan.FromSeconds(90));

    var tasks = targets.Select(async t =>
    {
        try
        {
            var resp = await client.GetAsync(t.Url, overall.Token);
            return (t.Service, ok: resp.IsSuccessStatusCode);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Wake ping failed for {Service} ({Url})", t.Service, t.Url);
            return (t.Service, ok: false);
        }
    }).ToArray();

    var results = await Task.WhenAll(tasks);
    var allOk = results.All(r => r.ok);
    if (allOk)
    {
        WakeThrottle.MarkWoken();
    }

    return Results.Ok(new
    {
        status = allOk ? "woken" : "partial",
        services = results.ToDictionary(r => r.Service, r => r.ok),
    });
});

app.MapReverseProxy();

app.Run();
