using MicroBootstrap.Abstractions.Core.Domain.Events.Store;

namespace MicroBootstrap.Abstractions.Core.Domain.Model.EventSourcing;

public interface IEventSourcedAggregate<out TId> : IAggregate<TId>, IHaveEventSourcingAggregate
{
}

public interface IEventSourcedAggregate<out TIdentity, TId> : IEventSourcedAggregate<TIdentity>
    where TIdentity : Identity<TId>
{
}

public interface IEventSourcedAggregate : IEventSourcedAggregate<AggregateId, long>
{
}