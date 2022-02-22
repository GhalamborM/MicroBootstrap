using MediatR;

namespace MicroBootstrap.Abstractions.Domain.Events;

public interface IEventHandler<in TEvent> : INotificationHandler<TEvent>
    where TEvent : INotification
{
}
