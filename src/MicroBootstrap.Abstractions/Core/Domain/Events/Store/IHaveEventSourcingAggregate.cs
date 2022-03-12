using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store.Projections;
using MicroBootstrap.Abstractions.Core.Domain.Model;

namespace MicroBootstrap.Abstractions.Core.Domain.Events.Store;

public interface IHaveEventSourcingAggregate : IHaveAggregateStateProjection, IHaveAggregate
{
    /// <summary>
    /// Update aggregate state from the events in the event store and increase the current version and original version.
    /// </summary>
    /// <param name="domainEvents">Domain events from the aggregate stream.</param>
    void LoadFromHistory(IEnumerable<IDomainEvent> domainEvents);
}