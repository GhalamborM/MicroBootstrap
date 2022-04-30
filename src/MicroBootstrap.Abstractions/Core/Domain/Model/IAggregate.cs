using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;

namespace MicroBootstrap.Abstractions.Core.Domain.Model;

public interface IAggregate<out TId> : IEntity<TId>, IHaveAggregate
{
    /// <summary>
    /// Check specific rule for aggregate and throw an exception if rule is not satisfied.
    /// </summary>
    /// <param name="rule"></param>
    void CheckRule(IBusinessRule rule);

    /// <summary>
    /// Gets get the list of pending changes domain events to be applied to the aggregate.
    /// </summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
}

public interface IAggregate<out TIdentity, TId> : IAggregate<TIdentity>
    where TIdentity : Identity<TId>
{
}

public interface IAggregate : IAggregate<AggregateId, long>
{
}