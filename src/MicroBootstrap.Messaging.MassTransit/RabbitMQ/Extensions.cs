using System.Reflection;
using GreenPipes;
using Humanizer;
using MassTransit;
using MassTransit.Definition;
using MassTransit.RabbitMqTransport;
using MassTransit.RabbitMqTransport.Topology;
using MassTransit.Topology;
using MicroBootstrap.Abstractions.Core.Domain.Events.External;
using MicroBootstrap.Core.Extensions.Configuration;
using MicroBootstrap.Messaging.MassTransit.RabbitMQ.Options;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace MicroBootstrap.Messaging.MassTransit.RabbitMQ;

// https://www.youtube.com/watch?v=bsUlQ93j2MY
// https://masstransit-project.com/advanced/topology/message.html
// https://masstransit-project.com/advanced/topology/rabbitmq.html
// https://masstransit-project.com/advanced/topology/conventions.html
public static class Extensions
{
    public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services)
    {
        services.AddMassTransit(configure =>
        {
            configure.SetKebabCaseEndpointNameFormatter();

            // exclude namespace for the messages
            configure.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(false));

            // https://masstransit-project.com/usage/configuration.html#asp-net-core
            // https://masstransit-project.com/usage/containers/msdi.html
            configure.AddConsumers(Assembly.GetEntryAssembly());

            configure.UsingRabbitMq((context, configurator) =>
            {
                var configuration = context.GetService<IConfiguration>();
                var rabbitmqOptions =
                    configuration.GetOptions<MassTransitRabbitMQOptions>(nameof(MassTransitRabbitMQOptions));

                configurator.Host(rabbitmqOptions.HostName);

                configurator.ConfigureEndpoints(context);

                var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                    .Where(x => x.IsAssignableTo(typeof(IIntegrationEvent)) && x.IsInterface == false &&
                                x.IsAbstract == false &&
                                x.IsGenericType == false);

                foreach (var type in types)
                {
                    var consumers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                        .Where(x => x.IsAssignableTo(typeof(IConsumer<>).MakeGenericType(type))).ToList();

                    //////////
                    var messageMethodInfo = configurator.GetType().GetMethod("Message");
                    MethodInfo messageMethodInfoGeneric = messageMethodInfo.MakeGenericMethod(type);
                    var messageTopologyMethodType = typeof(Extensions).GetMethods()
                        .FirstOrDefault(x => x.Name == nameof(CreateMessageTopologyConfigurator))
                        ?.MakeGenericMethod(type);

                    var messageMethodDelegate = messageTopologyMethodType.Invoke(null, null);

                    // prepare delegate instance
                    messageMethodInfoGeneric.Invoke(configurator, new object?[] { messageMethodDelegate });

                    /////
                    var publishMethodInfo = typeof(IRabbitMqBusFactoryConfigurator)
                        .GetMethod("Publish");
                    MethodInfo publishMethodInfoGeneric = publishMethodInfo.MakeGenericMethod(type);

                    var publishTopologyMethodType = typeof(Extensions).GetMethods()
                        .FirstOrDefault(x => x.Name == nameof(CreatePublishTopologyConfigurator))
                        ?.MakeGenericMethod(type);
                    var publishMethodDelegate = publishTopologyMethodType.Invoke(null, null);

                    // prepare delegate instance
                    publishMethodInfoGeneric.Invoke(configurator, new object?[] { publishMethodDelegate });

                    ////////
                    configurator.ReceiveEndpoint(type.Name.Underscore(), e =>
                    {
                        e.ExchangeType = ExchangeType.Topic;
                        e.UseMessageRetry(retryConfigurator =>
                            retryConfigurator.Interval(3, TimeSpan.FromSeconds(5.0)));

                        // https:// masstransit-project.com/usage/configuration.html#asp-net-core
                        foreach (var consumer in consumers)
                        {
                            var methodInfo = typeof(DependencyInjectionReceiveEndpointExtensions)
                                .GetMethods()
                                .Where(x => x.GetParameters().Any(p => p.ParameterType == typeof(IServiceProvider)))
                                .FirstOrDefault(x => x.Name == "Consumer" && x.IsGenericMethod);

                            MethodInfo generic = methodInfo.MakeGenericMethod(consumer);
                            generic.Invoke(e, new object?[] { e, context, null });
                        }
                    });
                }

                configurator.UseMessageRetry(retryConfigurator =>
                {
                    retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                });
            });
        });

        services.AddMassTransitHostedService();

        return services;
    }

    // https://stackoverflow.com/questions/19302513/using-reflection-to-invoke-generic-method-passing-lambda-expression
    private static Action<IRabbitMqMessagePublishTopologyConfigurator<T>> CreatePublishTopologyConfigurator<T>()
        where T : class
    {
        return new Action<IRabbitMqMessagePublishTopologyConfigurator<T>>((param) =>
        {
            param.ExchangeType = ExchangeType.Topic;
        });
    }

    private static Action<IMessageTopologyConfigurator<T>> CreateMessageTopologyConfigurator<T>()
        where T : class
    {
        return new Action<IMessageTopologyConfigurator<T>>((param) =>
        {
            param.SetEntityName(typeof(T).Name.Underscore());
        });
    }
}
