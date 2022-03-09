namespace MicroBootstrap.Abstractions.Core.Domain.Events.Internal;

/// <summary>
/// The domain event interface.
/// </summary>
public interface IDomainEvent : IEvent
{
    /// <summary>
    /// Gets the identifier of the aggregate which has generated the event.
    /// </summary>
    dynamic AggregateId { get; }

    public IDomainEvent WithAggregate(dynamic aggregateId, long version);
}
