using MicroBootstrap.Abstractions.Domain.Events.Internal;

namespace MicroBootstrap.Abstractions.Domain.Model;

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
    public IReadOnlyList<IDomainEvent> FlushUncommittedEvents();

    public IReadOnlyList<IDomainEvent> GetUncommittedEvents();
}
