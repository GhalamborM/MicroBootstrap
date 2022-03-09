using RabbitMQ.Client;

namespace MicroBootstrap.Messaging.Transport.Rabbitmq;

public interface IBusConnection
{
    bool IsConnected { get; }
    IModel CreateChannel();
}
