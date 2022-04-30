using MicroBootstrap.Abstractions.Core.Domain.Events.External;

namespace MicroBootstrap.Messaging.Transport.Rabbitmq;

public interface IPublisherChannelFactory
{
    PublisherChannelContext Create(IIntegrationEvent message);
}
