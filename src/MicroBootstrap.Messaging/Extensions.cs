using System.Reflection;
using MicroBootstrap.Messaging.BackgroundServices;
using MicroBootstrap.Messaging.Outbox.EF;
using MicroBootstrap.Messaging.Outbox.InMemory;
using MicroBootstrap.Messaging.Serialization.Newtonsoft;

namespace MicroBootstrap.Messaging;

public static class Extensions
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<NewtonsoftJsonUnSupportedTypeMatcher>();
        services.AddSingleton<IMessageSerializer, NewtonsoftJsonMessageSerializer>();

        services.AddScoped<IMessageDispatcher, MessageDispatcher>();

        services.AddHostedService<SubscribersBackgroundService>();
        services.AddHostedService<ConsumerBackgroundWorker>();
        services.AddHostedService<OutboxProcessorBackgroundService>();

        var typeResolver = new TypeResolver();
        services.AddSingleton<ITypeResolver>(typeResolver);
        RegisterIntegrationMessagesToTypeResolver(typeResolver);

        return services;
    }

    public static IServiceCollection AddEntityFrameworkOutbox<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly migrationAssembly)
        where TContext : EfDbContextBase
    {
        var outboxOption = Guard.Against.Null(
            configuration.GetOptions<OutboxOptions>(nameof(OutboxOptions)),
            nameof(OutboxOptions));

        services.AddOptions<OutboxOptions>().Bind(configuration.GetSection(nameof(OutboxOptions)))
            .ValidateDataAnnotations();

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddDbContext<TContext>(options =>
        {
            options.UseNpgsql(outboxOption.ConnectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(migrationAssembly.GetName().Name);
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            }).UseSnakeCaseNamingConvention();
        });

        services.AddUnitOfWork<TContext>();
        services.AddScoped<IOutboxService, EfOutboxService<TContext>>();

        return services;
    }

    public static IServiceCollection AddInMemoryOutbox(this IServiceCollection services)
    {
        services.AddSingleton<IInMemoryOutboxStore, InMemoryOutboxStore>();
        services.AddScoped<IOutboxService, InMemoryOutboxService>();

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
}
