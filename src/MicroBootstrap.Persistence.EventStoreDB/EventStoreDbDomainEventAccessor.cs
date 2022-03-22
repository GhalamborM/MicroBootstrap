using MicroBootstrap.Abstractions.Core.Domain.Events;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;

namespace MicroBootstrap.Persistence.EventStoreDB;

public class EventStoreDbDomainEventAccessor : IDomainEventsAccessor
{
    private readonly IAggregatesDomainEventsRequestStore _aggregatesDomainEventsStore;

    public EventStoreDbDomainEventAccessor(IAggregatesDomainEventsRequestStore aggregatesDomainEventsStore)
    {
        _aggregatesDomainEventsStore = aggregatesDomainEventsStore;
    }

    public IReadOnlyList<IDomainEvent> UnCommittedDomainEvents =>
        _aggregatesDomainEventsStore.GetAllUncommittedEvents();
}