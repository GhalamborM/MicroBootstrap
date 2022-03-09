using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Core.Domain.Events.External;

namespace MicroBootstrap.Core.Domain.Events.Internal;

public record IntegrationEventWrapper<TDomainEventType>(TDomainEventType DomainEvent) : IntegrationEvent
    where TDomainEventType : IDomainEvent;
