namespace MicroBootstrap.Abstractions.Core.Domain.Events.Internal;

public interface IDomainEventHandler<in TEvent> : IEventHandler<TEvent>
    where TEvent : IDomainEvent
{
}
