using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Model;

namespace MicroBootstrap.Abstractions.Core.Domain.Events;

public interface IAggregatesDomainEventsStore
{
    IReadOnlyList<IDomainEvent> AddEventsFromAggregate<T>(T aggregate)
        where T : IHaveAggregate;

    void AddEvents(IReadOnlyList<IDomainEvent> events);

    IReadOnlyList<IDomainEvent> GetAllUncommittedEvents();
}