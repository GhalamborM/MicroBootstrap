namespace MicroBootstrap.Messaging.Transport.Rabbitmq;

public interface IMessageParser
{
    IIntegrationEvent Resolve(IBasicProperties basicProperties, byte[] body);
}
