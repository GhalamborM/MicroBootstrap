using MicroBootstrap.Abstractions.Messaging.Outbox;
using MicroBootstrap.Messaging.InMemory.Outbox;

namespace MicroBootstrap.Messaging.InMemory;

public static class Extensions
{
    public static IServiceCollection AddInMemoryOutbox(this IServiceCollection services)
    {
        services.AddSingleton<IInMemoryOutboxStore, InMemoryOutboxStore>();
        services.AddScoped<IOutboxService, InMemoryOutboxService>();

        return services;
    }
}