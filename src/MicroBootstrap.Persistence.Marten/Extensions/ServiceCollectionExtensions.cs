using Marten;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using MicroBootstrap.Abstractions.Core;
using MicroBootstrap.Abstractions.Core.Domain.Events;
using MicroBootstrap.Abstractions.Persistence.EventStore;
using MicroBootstrap.Core.Extensions.Configuration;
using MicroBootstrap.Core.Extensions.DependencyInjection;
using MicroBootstrap.Core.Extensions.Registration;
using MicroBootstrap.Persistence.Marten.Subscriptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Weasel.Core;

namespace MicroBootstrap.Persistence.Marten.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMarten(
        this IServiceCollection services,
        IConfiguration configuration,
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        Action<StoreOptions>? configureOptions = null
    )

    {
        var martenOptions = configuration.GetOptions<MartenOptions>(nameof(MartenOptions));

        if (configureOptions is { })
        {
            services.Configure(nameof(MartenOptions), configureOptions);
        }
        else
        {
            services.AddOptions<MartenOptions>().Bind(configuration.GetSection(nameof(MartenOptions)))
                .ValidateDataAnnotations();
        }

        services
            .AddScoped<IIdGenerator<Guid>, MartenIdGenerator>()
            .AddMarten(sp => SetStoreOptions(sp, martenOptions, configureOptions))
            .ApplyAllDatabaseChangesOnStartup()
            .AddAsyncDaemon(DaemonMode.Solo);

        services.Add<IAggregateStore, MartenAggregateStore>(lifetime);
        services.Add<IDomainEventsAccessor, MartenDomainEventAccessor>(lifetime);
        services.AddEventStore<MartenEventStore>(lifetime);

        return services;
    }

    private static StoreOptions SetStoreOptions(
        IServiceProvider serviceProvider,
        MartenOptions martenOptions,
        Action<StoreOptions>? configureOptions = null
    )
    {
        var options = new StoreOptions();
        options.Connection(martenOptions.ConnectionString);
        options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;

        var schemaName = Environment.GetEnvironmentVariable("SchemaName");
        options.Events.DatabaseSchemaName = schemaName ?? martenOptions.WriteModelSchema;
        options.DatabaseSchemaName = schemaName ?? martenOptions.ReadModelSchema;

        options.UseDefaultSerialization(
            EnumStorage.AsString,
            nonPublicMembersStorage: NonPublicMembersStorage.All
        );

        options.Projections.Add(
            new MartenSubscription(
                new[] { new MartenEventPublisher(serviceProvider) },
                serviceProvider.GetRequiredService<ILogger<MartenSubscription>>()
            ),
            ProjectionLifecycle.Async,
            "MartenSubscription"
        );

        if (martenOptions.UseMetadata)
        {
            options.Events.MetadataConfig.CausationIdEnabled = true;
            options.Events.MetadataConfig.CorrelationIdEnabled = true;
        }

        configureOptions?.Invoke(options);

        return options;
    }
}