namespace MicroBootstrap.Messaging.Transport.Rabbitmq;

public interface IPublisherChannelContextPool
{
    PublisherChannelContext Get(QueueReferences references);
    void Return(PublisherChannelContext ctx);
}
