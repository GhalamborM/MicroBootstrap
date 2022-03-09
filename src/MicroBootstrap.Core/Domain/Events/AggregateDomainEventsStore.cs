using MicroBootstrap.Abstractions.Core.Domain.Events;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Model;

namespace MicroBootstrap.Core.Domain.Events;

public class AggregatesDomainEventsStore : IAggregatesDomainEventsStore
{
    private readonly List<IDomainEvent> _uncommittedDomainEvents = new();

    public IReadOnlyList<IDomainEvent> AddEventsFrom<T>(T aggregate)
        where T : IHaveAggregate
    {
        var events = aggregate.GetUncommittedEvents();

        if (events.Any())
        {
            _uncommittedDomainEvents.AddRange(events);
        }

        return events;
    }

    public IReadOnlyList<IDomainEvent> AddEventsFrom(object entity)
    {
        if (entity is IHaveAggregate aggregate)
        {
            return AddEventsFrom(aggregate);
        }

        return new List<IDomainEvent>();
    }

    public IReadOnlyList<IDomainEvent> GetAllUncommittedEvents()
    {
        return _uncommittedDomainEvents;
    }
}
