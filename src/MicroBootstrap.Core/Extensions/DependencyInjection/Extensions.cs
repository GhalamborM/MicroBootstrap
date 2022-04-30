using System.Reflection;
using MicroBootstrap.Abstractions.Core;
using MicroBootstrap.Abstractions.Core.Domain.Events;
using MicroBootstrap.Abstractions.Core.Domain.Events.External;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Messaging;
using MicroBootstrap.Abstractions.Messaging.Serialization;
using MicroBootstrap.Abstractions.Messaging.Transport;
using MicroBootstrap.Abstractions.Persistence.EventStore;
using MicroBootstrap.Abstractions.Persistence.EventStore.Projections;
using MicroBootstrap.Abstractions.Types;
using MicroBootstrap.Core.Domain.Events;
using MicroBootstrap.Core.Extensions.Registration;
using MicroBootstrap.Core.Extensions.Utils.Reflections;
using MicroBootstrap.Core.IdsGenerator;
using MicroBootstrap.Core.Persistence.EventStore;
using MicroBootstrap.Core.Persistence.EventStore.InMemory;
using MicroBootstrap.Core.Serialization.Newtonsoft;
using MicroBootstrap.Core.Types;
using MicroBootstrap.Messaging;
using MicroBootstrap.Messaging.BackgroundServices;
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

        AddEventStore<InMemoryEventStore>(services, ServiceLifetime.Singleton);

        AddDefaultMessageSerializer(services, ServiceLifetime.Transient);

        AddMessagingCore(services);
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

    private static IServiceCollection AddMessagingCore(this IServiceCollection services)
    {
        services.AddScoped<IMessageDispatcher, MessageDispatcher>();
        services.AddHostedService<SubscribersBackgroundService>();
        services.AddHostedService<ConsumerBackgroundWorker>();
        services.AddHostedService<OutboxProcessorBackgroundService>();

        var typeResolver = new TypeResolver();
        services.AddSingleton<ITypeResolver>(typeResolver);
        RegisterIntegrationMessagesToTypeResolver(typeResolver);

        return services;
    }

    private static void RegisterIntegrationMessagesToTypeResolver(
        ITypeResolver typeResolver)
    {
        Console.WriteLine("preloading all message types...");

        var messageType = typeof(IIntegrationEvent);

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var types = assemblies.SelectMany(x => x.GetTypes())
            .Where(type =>
                messageType.IsAssignableFrom(type) && type.IsInterface == false && type.IsAbstract == false)
            .Distinct()
            .ToList();

        typeResolver.Register(types);

        Console.WriteLine("preloading all message types completed!");
    }

    public static IEnumerable<Type> GetHandledMessageTypes(params Assembly[] assemblies)
    {
        var messageHandlerTypes = typeof(IMessageHandler<>).GetAllTypesImplementingOpenGenericInterface(assemblies)
            .ToList();

        var inheritsTypes = messageHandlerTypes.SelectMany(x => x.GetInterfaces())
            .Where(x => x.GetInterfaces().Any(i => i.IsGenericType) &&
                        x.GetGenericTypeDefinition() == typeof(IMessageHandler<>));

        foreach (var inheritsType in inheritsTypes)
        {
            var messageType = inheritsType.GetGenericArguments().First();
            if (messageType.IsAssignableTo(typeof(IMessage)))
            {
                yield return messageType;
            }
        }
    }

    public static IServiceCollection AddBusSubscriber(this IServiceCollection services, Type subscriberType)
    {
        if (services.All(s => s.ImplementationType != subscriberType))
            services.AddSingleton(typeof(IEventBusSubscriber), subscriberType);
        return services;
    }

    private static void AddDefaultMessageSerializer(IServiceCollection services, ServiceLifetime lifetime)
    {
        services.Add<IMessageSerializer, NewtonsoftJsonMessageSerializer>(lifetime);
    }

    public static IServiceCollection AddEventStore<TEventStore>(
        this IServiceCollection services,
        ServiceLifetime withLifetime = ServiceLifetime.Scoped)
        where TEventStore : class, IEventStore
    {
        services.Add<IAggregatesDomainEventsRequestStore, AggregatesDomainEventsRequestStore>(withLifetime);
        services.Add<IAggregateStore, AggregateStore>(withLifetime);

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
            .AddClasses(classes => classes.AssignableTo<IHaveReadProjection>()) // Filter classes
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