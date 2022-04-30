using MediatR;

namespace MicroBootstrap.Abstractions.Core.Domain.Events;

public interface IEventHandler<in TEvent> : INotificationHandler<TEvent>
    where TEvent : INotification
{
}
