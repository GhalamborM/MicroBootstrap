using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store.Projections;

namespace MicroBootstrap.Abstractions.Core.Domain.Events.Store;

public interface IHaveEventSourcingAggregate : IHaveAggregateStateProjection
{
    string StreamName { get; }

    /// <summary>
    /// Load an aggregate from an enumerable of events.
    /// </summary>
    /// <param name="domainEvents"></param>
    void LoadFromHistory(IEnumerable<IDomainEvent> domainEvents);
}