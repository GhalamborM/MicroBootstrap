using System.Reflection;
using MicroBootstrap.Core.Events;
using MicroBootstrap.Core.Events.Store;
using MicroBootstrap.Core.IdsGenerator;
using MicroBootstrap.Core.Objects;

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

        switch (configuration.GetValue<string>("IdGenerator:Type"))
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

    public static IServiceCollection AddEventStorePipeline(
        this IServiceCollection services,
        ServiceLifetime withLifetime = ServiceLifetime.Transient)
    {
        return services.Add(typeof(INotificationHandler<>), typeof(EventStorePipeline<>), withLifetime);
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
