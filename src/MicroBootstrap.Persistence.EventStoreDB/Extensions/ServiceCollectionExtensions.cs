using EventStore.Client;
using MicroBootstrap.Core.Extensions.Configuration;
using MicroBootstrap.Core.Extensions.DependencyInjection;
using MicroBootstrap.Persistence.EventStoreDB.Subscriptions;
using Microsoft.Extensions.Configuration;

namespace MicroBootstrap.Persistence.EventStoreDB.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventStoreDb(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<EventStoreDbOptions>? configureOptions = null)
    {
        var eventStoreDbConfig = configuration.GetOptions<EventStoreDbOptions>(nameof(EventStoreDbOptions));

        services.AddSingleton(
            new EventStoreClient(EventStoreClientSettings.Create(eventStoreDbConfig.ConnectionString)));

        services.AddEventStore<EventStoreDbEventStore>(ServiceLifetime.Scoped);

        if (eventStoreDbConfig.UseInternalCheckpointing)
        {
            services.AddTransient<ISubscriptionCheckpointRepository, EventStoreDbSubscriptionCheckPointRepository>();
        }

        if (configureOptions is { })
        {
            services.Configure(nameof(EventStoreDbOptions), configureOptions);
        }
        else
        {
            services.AddOptions<EventStoreDbOptions>().Bind(configuration.GetSection(nameof(EventStoreDbOptions)))
                .ValidateDataAnnotations();
        }

        return services;
    }

    public static IServiceCollection AddEventStoreDBSubscriptionToAll(
        this IServiceCollection services,
        bool checkpointToEventStoreDb = true)
    {
        if (checkpointToEventStoreDb)
        {
            services
                .AddTransient<ISubscriptionCheckpointRepository, EventStoreDbSubscriptionCheckPointRepository>();
        }

        return services.AddHostedService<EventStoreDBSubscriptionToAll>();
    }
}