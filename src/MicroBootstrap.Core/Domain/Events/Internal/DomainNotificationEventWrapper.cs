using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;

namespace MicroBootstrap.Core.Domain.Events.Internal;

public record DomainNotificationEventWrapper<TDomainEventType>(TDomainEventType DomainEvent) : DomainNotificationEvent
    where TDomainEventType : IDomainEvent;
