using System.Reflection;
using MicroBootstrap.Abstractions.Core;
using MicroBootstrap.Abstractions.Core.Domain.Events;
using MicroBootstrap.Abstractions.Core.Domain.Events.External;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store.Projections;
using MicroBootstrap.Abstractions.Types;
using MicroBootstrap.Core.Domain.Events;
using MicroBootstrap.Core.Domain.Events.Store;
using MicroBootstrap.Core.Domain.Events.Store.InMemory;
using MicroBootstrap.Core.Extensions.Registration;
using MicroBootstrap.Core.IdsGenerator;
using MicroBootstrap.Core.Types;
using Microsoft.Extensions.Configuration;

namespace MicroBootstrap.Core.Extensions.DependencyInjection;

public static class Extensions
{
    public static IServiceCollection AddCore(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[]? assembliesToScan)
    {
        var systemInfo = SystemInfo.New();

        services.AddSingleton<ISystemInfo>(systemInfo);
        services.AddSingleton(systemInfo);

        services.AddScoped<IEventProcessor, EventProcessor>();
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();
        services.AddScoped<IIntegrationEventPublisher, IntegrationEventPublisher>();
        services.AddScoped<IDomainNotificationEventPublisher, DomainNotificationEventPublisher>();
        services.AddScoped<IAggregatesDomainEventsStore, AggregatesDomainEventsStore>();

        AddEventStore<InMemoryEventStore>(services, ServiceLifetime.Scoped);
        services.AddScoped<IEventStoreRepository, EventStoreRepository>();

        switch (configuration["IdGenerator:Type"])
        {
            case "Guid":
                services.AddSingleton<IIdGenerator<Guid>, GuidIdGenerator>();
                break;
            default:
                services.AddSingleton<IIdGenerator<long>, SnowFlakIdGenerator>();
                break;
        }

        RegisterEventMappers(services, assembliesToScan);

        return services;
    }

    public static IServiceCollection AddEventStore<TEventStore>(
        this IServiceCollection services,
        ServiceLifetime withLifetime = ServiceLifetime.Transient)
        where TEventStore : class, IEventStore
    {
        return services.Add<TEventStore, TEventStore>(withLifetime)
            .Add<IEventStore>(sp => sp.GetRequiredService<TEventStore>(), withLifetime);
    }

    public static IServiceCollection AddReadProjections(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddSingleton<IReadProjectionPublisher, ReadProjectionPublisher>();
        var assembliesToScan = assemblies.Any() ? assemblies : new[] { Assembly.GetEntryAssembly() };

        RegisterProjections(services, assembliesToScan!);

        return services;
    }

    private static void RegisterProjections(IServiceCollection services, Assembly[] assembliesToScan)
    {
        services.Scan(scan => scan
            .FromAssemblies(assembliesToScan)
            .AddClasses(classes => classes.AssignableTo<IReadProjection>()) // Filter classes
            .AsImplementedInterfaces()
            .WithTransientLifetime());
    }

    private static void RegisterEventMappers(IServiceCollection services, params Assembly[]? assembliesToScan)
    {
        services.Scan(scan => scan
            .FromAssemblies(assembliesToScan ?? AppDomain.CurrentDomain.GetAssemblies())
            .AddClasses(classes => classes.AssignableTo(typeof(IEventMapper)), false)
            .AddClasses(classes => classes.AssignableTo(typeof(IIntegrationEventMapper)), false)
            .AddClasses(classes => classes.AssignableTo(typeof(IIDomainNotificationEventMapper)), false)
            .AsImplementedInterfaces()
            .WithSingletonLifetime());
    }
}