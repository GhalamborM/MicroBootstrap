namespace MicroBootstrap.Abstractions.Persistence.EventStore.Projections;

public interface IHaveAggregateStateProjection
{
    /// <summary>
    /// Update the aggregate state for new events that are added to the event store and also for events that are already in the event store without increasing the version.
    /// </summary>
    /// <param name="event"></param>
    void When(object @event);

    /// <summary>
    /// Update the aggregate state for new events that are loaded form the event store and increase the current version and original version.
    /// </summary>
    /// <param name="event"></param>
    void Fold(object @event);
}