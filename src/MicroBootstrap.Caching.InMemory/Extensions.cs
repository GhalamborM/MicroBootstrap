using System.Reflection;
using Ardalis.GuardClauses;
using MicroBootstrap.Abstractions.Caching;
using MicroBootstrap.Core.Caching;
using MicroBootstrap.Core.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicroBootstrap.Caching.InMemory;

public static class Extensions
{
    public static IServiceCollection AddCustomInMemoryCache(
        this IServiceCollection services,
        IConfiguration config,
        Action<InMemoryCacheOptions>? configureOptions = null)
    {
        Guard.Against.Null(services, nameof(services));

        var options = config.GetOptions<InMemoryCacheOptions>(nameof(InMemoryCacheOptions));

        if (configureOptions is { })
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.AddOptions<InMemoryCacheOptions>().Bind(config.GetSection(nameof(InMemoryCacheOptions)))
                .ValidateDataAnnotations();
        }

        services.AddMemoryCache();
        services.AddTransient<ICacheManager, CacheManager>();
        services.AddSingleton<ICacheProvider, InMemoryCacheProvider>();

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
