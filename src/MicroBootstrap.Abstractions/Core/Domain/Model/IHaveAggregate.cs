using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;

namespace MicroBootstrap.Abstractions.Core.Domain.Model;

public interface IHaveAggregate
{
    /// <summary>
    /// Add the <paramref name="domainEvent"/> on the aggregate root.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    void AddDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// Returns all uncommitted events and clears this events from the aggregate.
    /// </summary>
    /// <returns>Array of new uncommitted events.</returns>
    IReadOnlyList<IDomainEvent> FlushUncommittedEvents();

    /// <summary>
    /// Return all uncommitted events.
    /// </summary>
    /// <returns></returns>
    IReadOnlyList<IDomainEvent> GetUncommittedEvents();

    /// <summary>
    /// Clear all uncommitted events.
    /// </summary>
    void ClearUncommittedEvents();
}
