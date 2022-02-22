using MicroBootstrap.Abstractions.Domain.Events.External;
using MicroBootstrap.Abstractions.Domain.Events.Internal;

namespace MicroBootstrap.Abstractions.Domain.Events;

public interface IEventMapper : IIDomainNotificationEventMapper, IIntegrationEventMapper
{
}

public interface IIDomainNotificationEventMapper
{
    IReadOnlyList<IDomainNotificationEvent?>? MapToDomainNotificationEvents(IReadOnlyList<IDomainEvent> domainEvents);
    IDomainNotificationEvent? MapToDomainNotificationEvent(IDomainEvent domainEvent);
}

public interface IIntegrationEventMapper
{
    IReadOnlyList<IIntegrationEvent?>? MapToIntegrationEvents(IReadOnlyList<IDomainEvent> domainEvents);
    IIntegrationEvent? MapToIntegrationEvent(IDomainEvent domainEvent);
}
