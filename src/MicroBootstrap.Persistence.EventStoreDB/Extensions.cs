using MicroBootstrap.Persistence.EventStoreDB.Subscriptions;

namespace MicroBootstrap.Persistence.EventStoreDB;

public static class Extensions
{
    public static IServiceCollection AddEventStore(
        this IServiceCollection services,
        IConfiguration configuration,
        EventStoreDbOptions? options = null)
    {
        var eventStoreDbConfig = configuration.GetOptions<EventStoreDbOptions>(nameof(EventStoreDbOptions));

        services.AddSingleton(
            new EventStoreClient(EventStoreClientSettings.Create(eventStoreDbConfig.ConnectionString)));

        if (options?.UseInternalCheckpointing != false)
        {
            services
                .AddTransient<ISubscriptionCheckpointRepository, EventStoreDbSubscriptionCheckPointRepository>();
        }

        return services;
    }

    public static IServiceCollection AddEventStoreDbSubscriptionToAll(
        this IServiceCollection services,
        string subscriptionId,
        SubscriptionFilterOptions? filterOptions = null,
        Action<EventStoreClientOperationOptions>? configureOperation = null,
        UserCredentials? credentials = null,
        bool checkpointToEventStoreDb = true)
    {
        if (checkpointToEventStoreDb)
        {
            services
                .AddTransient<ISubscriptionCheckpointRepository, EventStoreDbSubscriptionCheckPointRepository>();
        }

        return services.AddHostedService(serviceProvider =>
            new SubscribeToAllBackgroundWorker(
                serviceProvider,
                serviceProvider.GetRequiredService<EventStoreClient>(),
                serviceProvider.GetRequiredService<ISubscriptionCheckpointRepository>(),
                serviceProvider.GetRequiredService<ILogger<SubscribeToAllBackgroundWorker>>(),
                subscriptionId,
                filterOptions,
                configureOperation,
                credentials
            )
        );
    }
}
