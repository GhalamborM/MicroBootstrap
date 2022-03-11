using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store.Projections;
using MicroBootstrap.Abstractions.Core.Domain.Model;

namespace MicroBootstrap.Abstractions.Core.Domain.Events.Store;

public interface IHaveEventSourcingAggregate : IHaveAggregateStateProjection, IHaveAggregate
{
    string StreamName { get; }

    /// <summary>
    /// Load an aggregate from an enumerable of events.
    /// </summary>
    /// <param name="domainEvents"></param>
    void LoadFromHistory(IEnumerable<IDomainEvent> domainEvents);
}