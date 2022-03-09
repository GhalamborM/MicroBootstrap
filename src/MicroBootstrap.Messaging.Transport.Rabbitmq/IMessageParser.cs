using MicroBootstrap.Abstractions.Core.Domain.Events.External;
using RabbitMQ.Client;

namespace MicroBootstrap.Messaging.Transport.Rabbitmq;

public interface IMessageParser
{
    IIntegrationEvent Resolve(IBasicProperties basicProperties, byte[] body);
}
