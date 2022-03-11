using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;

namespace MicroBootstrap.Abstractions.Core.Domain.Model;

public interface IAggregate<out TId> : IEntity<TId>, IHaveAggregate
{
    void CheckRule(IBusinessRule rule);
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
}

public interface IAggregate<out TIdentity, TId> : IAggregate<TIdentity>
    where TIdentity : Identity<TId>
{
}

public interface IAggregate : IAggregate<AggregateId, long>
{
}