namespace MicroBootstrap.Messaging.Transport.Rabbitmq;

public interface IQueueReferenceFactory
{
    QueueReferences Create<TM>(TM message = default)
        where TM : IIntegrationEvent;
}
