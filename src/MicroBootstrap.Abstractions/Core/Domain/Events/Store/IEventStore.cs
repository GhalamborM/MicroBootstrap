namespace MicroBootstrap.Abstractions.Core.Domain.Events.Store;

public interface IEventStore
{
    /// <summary>
    /// Save events.
    /// </summary>
    /// <param name="events">Events to be saved.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task of save operation.</returns>
    Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets events for an aggregate.
    /// </summary>
    /// <typeparam name="TAggregateId">Type of aggregate id.</typeparam>
    /// <param name="aggregateId">Guid of the aggregate to be retrieved.</param>
    /// <param name="fromVersion">All events after this should be returned. -1 if from start.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task with events for aggregate.</returns>
    Task<IEnumerable<IEvent>> GetAsync<TAggregateId>(
        TAggregateId aggregateId,
        int fromVersion,
        CancellationToken cancellationToken = default);
}