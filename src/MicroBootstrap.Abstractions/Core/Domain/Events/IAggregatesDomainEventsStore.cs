using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Model;

namespace MicroBootstrap.Abstractions.Core.Domain.Events;

public interface IAggregatesDomainEventsStore
{
    IReadOnlyList<IDomainEvent> AddEventsFrom<T>(T aggregate)
        where T : IHaveAggregate;

    IReadOnlyList<IDomainEvent> AddEventsFrom(object entity);

    IReadOnlyList<IDomainEvent> GetAllUncommittedEvents();
}
