using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;

namespace MicroBootstrap.Core.Domain.Events.Internal;

public abstract record DomainEvent : Event, IDomainEvent
{
    public dynamic AggregateId { get; protected set; } = null!;
    public long SequenceNumber { get; protected set; }

    public virtual IDomainEvent WithAggregate(dynamic aggregateId, long version)
    {
        AggregateId = aggregateId;
        SequenceNumber = version;

        return this;
    }
}
