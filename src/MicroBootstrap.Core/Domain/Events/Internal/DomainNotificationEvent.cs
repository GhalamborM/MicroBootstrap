using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;

namespace MicroBootstrap.Core.Domain.Events.Internal;

public abstract record DomainNotificationEvent : Event, IDomainNotificationEvent;
