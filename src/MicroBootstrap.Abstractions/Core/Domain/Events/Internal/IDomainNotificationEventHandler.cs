namespace MicroBootstrap.Abstractions.Core.Domain.Events.Internal;

public interface IDomainNotificationEventHandler<in TEvent> : IEventHandler<TEvent>
    where TEvent : IDomainNotificationEvent

{
}
