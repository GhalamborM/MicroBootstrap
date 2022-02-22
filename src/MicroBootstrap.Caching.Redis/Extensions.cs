using System.Reflection;

namespace MicroBootstrap.Caching.Redis;

public static class Extensions
{
    public static IServiceCollection AddCustomRedisCache(
        this IServiceCollection services,
        IConfiguration config,
        Action<RedisCacheOptions>? configureOptions = null)
    {
        Guard.Against.Null(services, nameof(services));

        var redisOptions = config.GetOptions<RedisCacheOptions>(nameof(RedisCacheOptions));

        if (configureOptions is { })
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.AddOptions<RedisCacheOptions>().Bind(config.GetSection(nameof(RedisCacheOptions)))
                .ValidateDataAnnotations();
        }

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisOptions.ConnectionString));

        services.AddSingleton<ICacheManager, CacheManager>();
        services.AddSingleton<ICacheProvider, RedisCacheProvider>();

        return services;
    }

    public static IServiceCollection AddCachingRequestPolicies(
        this IServiceCollection services,
        params Assembly[] assembliesToScan)
    {
        // ICachePolicy discovery and registration
        services.Scan(scan => scan
            .FromAssemblies(assembliesToScan.Any() ? assembliesToScan : AppDomain.CurrentDomain.GetAssemblies())
            .AddClasses(
                classes => classes.AssignableTo(typeof(ICachePolicy<,>)),
                false)
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        // IInvalidateCachePolicy discovery and registration
        services.Scan(scan => scan
            .FromAssemblies(assembliesToScan.Any() ? assembliesToScan : AppDomain.CurrentDomain.GetAssemblies())
            .AddClasses(
                classes => classes.AssignableTo(typeof(IInvalidateCachePolicy<,>)),
                false)
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        return services;
    }
}
