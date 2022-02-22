namespace MicroBootstrap.Persistence.Marten;

public class MartenDomainEventAccessor : IDomainEventsAccessor
{
    private readonly IAggregatesDomainEventsStore _aggregatesDomainEventsStore;

    public MartenDomainEventAccessor(IAggregatesDomainEventsStore aggregatesDomainEventsStore)
    {
        _aggregatesDomainEventsStore = aggregatesDomainEventsStore;
    }

    public IReadOnlyList<IDomainEvent> UnCommittedDomainEvents
    {
        get
        {
            return _aggregatesDomainEventsStore.GetAllUncommittedEvents();
        }
    }
}
