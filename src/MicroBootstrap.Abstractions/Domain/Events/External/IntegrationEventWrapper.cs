using MicroBootstrap.Abstractions.Domain.Events.Internal;

namespace MicroBootstrap.Abstractions.Domain.Events.External;

public record IntegrationEventWrapper<TDomainEventType>(TDomainEventType DomainEvent) : IntegrationEvent
    where TDomainEventType : IDomainEvent;
