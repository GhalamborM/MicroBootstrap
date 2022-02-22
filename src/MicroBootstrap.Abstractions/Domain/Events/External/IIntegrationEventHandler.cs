namespace MicroBootstrap.Abstractions.Domain.Events.External;

public interface IIntegrationEventHandler<in TEvent> : IEventHandler<TEvent>
    where TEvent : IIntegrationEvent
{
}
