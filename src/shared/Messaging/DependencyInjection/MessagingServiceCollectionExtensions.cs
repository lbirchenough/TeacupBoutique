using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Messaging.Interfaces;
using Messaging.Options;
using Messaging.Services;
using Messaging.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Messaging.DependencyInjection;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddConsumer<T>(this IServiceCollection services)
        where T : class, IMessageConsumer
    {
        services.AddSingleton<T>();
        services.AddSingleton<IMessageConsumer>(sp => sp.GetRequiredService<T>());
        return services;
    }

    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration config)
    {
        var provider = config["Messaging:Provider"] ?? MessagingOptions.RabbitMq;

        if (string.Equals(provider, MessagingOptions.ServiceBus, StringComparison.OrdinalIgnoreCase))
        {
            services.Configure<ServiceBusOptions>(config.GetSection("ServiceBus"));

            services.AddSingleton(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<ServiceBusOptions>>().Value;
                if (!string.IsNullOrEmpty(opts.ConnectionString))
                    return new ServiceBusClient(opts.ConnectionString);
                if (!string.IsNullOrEmpty(opts.FullyQualifiedNamespace))
                    return new ServiceBusClient(opts.FullyQualifiedNamespace, new DefaultAzureCredential());
                throw new InvalidOperationException(
                    "ServiceBus config requires either ConnectionString (local dev) or FullyQualifiedNamespace (managed identity). Neither was set.");
            });

            services.AddSingleton<IMessagePublisher, ServiceBusPublisher>();
            services.AddHostedService<ServiceBusConsumerHost>();
        }
        else
        {
            services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
            services.AddHostedService<RabbitMqConsumerHost>();
        }

        return services;
    }
}
