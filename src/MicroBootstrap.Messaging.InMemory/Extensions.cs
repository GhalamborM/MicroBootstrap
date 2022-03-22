using MicroBootstrap.Abstractions.Messaging.Outbox;
using MicroBootstrap.Messaging.InMemory.Outbox;
using Microsoft.Extensions.Configuration;

namespace MicroBootstrap.Messaging.InMemory;

public static class Extensions
{
    public static IServiceCollection AddInMemoryMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddOutbox(services);

        return services;
    }

    private static IServiceCollection AddOutbox(this IServiceCollection services)
    {
        services.AddSingleton<IInMemoryOutboxStore, InMemoryOutboxStore>();
        services.AddScoped<IOutboxService, InMemoryOutboxService>();

        return services;
    }
}