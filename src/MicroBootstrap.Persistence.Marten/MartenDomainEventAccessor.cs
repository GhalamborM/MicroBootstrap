using MicroBootstrap.Abstractions.Core.Domain.Events;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;

namespace MicroBootstrap.Persistence.Marten;

public class MartenDomainEventAccessor : IDomainEventsAccessor
{
    private readonly IAggregatesDomainEventsRequestStore _aggregatesDomainEventsStore;

    public MartenDomainEventAccessor(IAggregatesDomainEventsRequestStore aggregatesDomainEventsStore)
    {
        _aggregatesDomainEventsStore = aggregatesDomainEventsStore;
    }

    public IReadOnlyList<IDomainEvent> UnCommittedDomainEvents =>
        _aggregatesDomainEventsStore.GetAllUncommittedEvents();
}