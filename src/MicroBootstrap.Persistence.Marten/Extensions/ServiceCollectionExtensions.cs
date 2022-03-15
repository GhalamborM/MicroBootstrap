using Marten;
using Marten.Events;
using MicroBootstrap.Abstractions.Core.Domain.Events;
using MicroBootstrap.Core.Extensions.Configuration;
using MicroBootstrap.Core.Extensions.DependencyInjection;
using MicroBootstrap.Core.Threading;
using Microsoft.Extensions.Configuration;
using Weasel.Core;

namespace MicroBootstrap.Persistence.Marten.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMarten(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<StoreOptions>? storeOptions = null,
        Action<MartenOptions>? configureOptions = null)
    {
        var martenOptions = configuration.GetOptions<MartenOptions>(nameof(MartenOptions));

        var documentStore = services
            .AddMarten(options => { SetStoreOptions(options, martenOptions, storeOptions); })
            .InitializeStore();

        SetupSchema(documentStore, martenOptions, 1);

        if (configureOptions is { })
        {
            services.Configure(nameof(MartenOptions), configureOptions);
        }
        else
        {
            services.AddOptions<MartenOptions>().Bind(configuration.GetSection(nameof(MartenOptions)))
                .ValidateDataAnnotations();
        }

        services.AddScoped<IDomainEventsAccessor, MartenDomainEventAccessor>();
        services.AddScoped<IMartenUnitOfWork, MartenUnitOfWork>();
        services.AddEventStore<MartenEventStore>(ServiceLifetime.Scoped);

        return services;
    }

    private static void SetupSchema(IDocumentStore documentStore, MartenOptions martenOptions, int retryLeft = 1)
    {
        try
        {
            if (martenOptions.ShouldRecreateDatabase)
                documentStore.Advanced.Clean.CompletelyRemoveAll();

            using (NoSynchronizationContextScope.Enter())
            {
                documentStore.Storage.ApplyAllConfiguredChangesToDatabaseAsync().Wait();
            }
        }
        catch
        {
            if (retryLeft == 0) throw;

            Thread.Sleep(1000);
            SetupSchema(documentStore, martenOptions, --retryLeft);
        }
    }

    private static void SetStoreOptions(
        StoreOptions options,
        MartenOptions martenOptions,
        Action<StoreOptions>? configureOptions = null)
    {
        options.Connection(martenOptions.ConnectionString);
        options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;

        var schemaName = Environment.GetEnvironmentVariable("SchemaName");
        options.Events.DatabaseSchemaName = schemaName ?? martenOptions.WriteModelSchema;
        options.DatabaseSchemaName = schemaName ?? martenOptions.ReadModelSchema;
        options.Events.StreamIdentity = StreamIdentity.AsString;

        options.UseDefaultSerialization(
            nonPublicMembersStorage: NonPublicMembersStorage.NonPublicSetters,
            enumStorage: EnumStorage.AsString);

        configureOptions?.Invoke(options);
    }
}