using MicroBootstrap.Abstractions.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Domain.Model;

namespace MicroBootstrap.Abstractions.Domain.Events;

public interface IAggregatesDomainEventsStore
{
    IReadOnlyList<IDomainEvent> AddEventsFrom<T>(T aggregate)
        where T : IHaveAggregate;

    IReadOnlyList<IDomainEvent> AddEventsFrom(object entity);

    IReadOnlyList<IDomainEvent> GetAllUncommittedEvents();
}
