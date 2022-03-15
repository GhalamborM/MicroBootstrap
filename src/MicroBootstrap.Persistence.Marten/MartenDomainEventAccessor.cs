using MicroBootstrap.Abstractions.Core.Domain.Events;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;

namespace MicroBootstrap.Persistence.Marten;

public class MartenDomainEventAccessor : IDomainEventsAccessor
{
    private readonly IAggregatesDomainEventsStore _aggregatesDomainEventsStore;

    public MartenDomainEventAccessor(IAggregatesDomainEventsStore aggregatesDomainEventsStore)
    {
        _aggregatesDomainEventsStore = aggregatesDomainEventsStore;
    }

    public IReadOnlyList<IDomainEvent> UnCommittedDomainEvents =>
        _aggregatesDomainEventsStore.GetAllUncommittedEvents();
}