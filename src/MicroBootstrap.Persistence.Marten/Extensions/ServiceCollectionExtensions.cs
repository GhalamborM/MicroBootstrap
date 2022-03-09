using Marten;
using Marten.Services;
using MicroBootstrap.Abstractions.Core.Domain.Events;
using MicroBootstrap.Core.Extensions.Configuration;
using MicroBootstrap.Core.Extensions.DependencyInjection;
using MicroBootstrap.Core.Extensions.Registration;
using MicroBootstrap.Core.Threading;
using Microsoft.Extensions.Configuration;
using Weasel.Core;

namespace MicroBootstrap.Persistence.Marten.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMarten(
        this IServiceCollection services,
        Func<IServiceProvider, string> getConnectionString,
        Action<StoreOptions>? setAdditionalOptions = null,
        string? schemaName = null,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient
    )
    {
        services.AddScoped(sp => CreateDocumentStore(getConnectionString(sp), setAdditionalOptions, schemaName));

        services.Add(
            sp =>
            {
                var store = sp.GetRequiredService<DocumentStore>();
                return CreateDocumentSession(store);
            },
            serviceLifetime);

        services.AddScoped<IDomainEventsAccessor, MartenDomainEventAccessor>();
        services.AddScoped<IMartenUnitOfWork, MartenUnitOfWork>();
        services.AddEventStore<MartenEventStore>(serviceLifetime);
        return services;
    }

    public static IServiceCollection AddMarten(
        this IServiceCollection services,
        IConfiguration config,
        Action<StoreOptions>? configureOptions = null,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient
    )
    {
        var martenOptions = config.GetOptions<MartenOptions>(nameof(MartenOptions));

        var documentStore = services
            .AddMarten(options =>
            {
                SetStoreOptions(options, martenOptions, configureOptions);
            })
            .InitializeStore();

        SetupSchema(documentStore, martenOptions, 1);

        services.AddScoped<IDomainEventsAccessor, MartenDomainEventAccessor>();
        services.AddScoped<IMartenUnitOfWork, MartenUnitOfWork>();
        services.AddEventStore<MartenEventStore>(serviceLifetime);

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
                documentStore.Schema.ApplyAllConfiguredChangesToDatabaseAsync().Wait();
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

        options.UseDefaultSerialization(
            nonPublicMembersStorage: NonPublicMembersStorage.NonPublicSetters,
            enumStorage: EnumStorage.AsString);

        // options.Projections.AsyncMode = config.DaemonMode;

        configureOptions?.Invoke(options);
    }


    public static DocumentStore CreateDocumentStore(
        string connectionString,
        Action<StoreOptions>? setAdditionalOptions = null,
        string? moduleName = null)
    {
        var store = DocumentStore.For(_ =>
        {
            _.Connection(connectionString);
            _.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;

            if (string.IsNullOrEmpty(moduleName) == false)
                _.DatabaseSchemaName = _.Events.DatabaseSchemaName = moduleName.ToLower();

            setAdditionalOptions?.Invoke(_);
        });

        return store;
    }

    public static IDocumentSession CreateDocumentSession(DocumentStore store)
    {
        var session = store.OpenSession(SessionOptions.ForCurrentTransaction());
        return session;
    }
}
