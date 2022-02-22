namespace MicroBootstrap.Messaging.Transport.Rabbitmq;

public interface IPublisherChannelFactory
{
    PublisherChannelContext Create(IIntegrationEvent message);
}
