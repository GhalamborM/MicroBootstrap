namespace MicroBootstrap.Abstractions.Core.Domain.Model;

public interface IAggregate<out TId> : IEntity<TId>, IHaveAggregate, IHaveVersion
{
    void CheckRule(IBusinessRule rule);
}

public interface IAggregate<out TIdentity, TId> : IAggregate<TIdentity>
    where TIdentity : Identity<TId>
{
}

public interface IAggregate : IAggregate<AggregateId, long>
{
}
