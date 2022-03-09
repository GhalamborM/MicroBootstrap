using MicroBootstrap.Abstractions.Messaging.Transport;
using MicroBootstrap.Messaging.Transport.InMemory.Channels;
using MicroBootstrap.Messaging.Transport.InMemory.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace MicroBootstrap.Messaging.Transport.InMemory
{
    public static class Extensions
    {
        public static IServiceCollection AddInMemoryTransport(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton<IEventBusPublisher, InMemoryPublisher>()
                .AddSingleton<IEventBusSubscriber, InMemorySubscriber>()
                .AddTransient<InMemoryProducerDiagnostics>()
                .AddTransient<InMemoryConsumerDiagnostics>();

            services.AddSingleton<IMessageChannel, MessageChannel>();
            return services;
        }
    }
}
