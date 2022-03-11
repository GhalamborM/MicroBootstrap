using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Model.EventSourcing;

namespace MicroBootstrap.Abstractions.Core.Domain.Events.Store;

public interface IEventStore
{
    /// <summary>
    /// Gets events for an specific streamId.
    /// </summary>
    /// <param name="streamId">Id of our aggregate or stream.</param>
    /// <param name="fromVersion">All events after this should be returned.</param>
    /// <param name="maxCount">Number of items to read.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task with events for aggregate.</returns>
    Task<IEnumerable<IStreamEvent>> GetAsync(
        string streamId,
        StreamReadPosition? fromVersion = null,
        long maxCount = long.MaxValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets events for an specific streamId.
    /// </summary>
    /// <param name="streamId">Id of our aggregate or stream.</param>
    /// <param name="fromVersion">All events after this should be returned.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task with events for aggregate.</returns>
    Task<IEnumerable<IStreamEvent>> GetAsync(
        string streamId,
        StreamReadPosition? fromVersion = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Append event to aggregate with no stream.
    /// </summary>
    /// <param name="streamId">Id of our aggregate or stream.</param>
    /// <param name="event">domain event to append the aggregate.</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEvent"></typeparam>
    /// <returns></returns>
    Task<AppendResult> AppendAsync<TEvent>(
        string streamId,
        IStreamEvent<TEvent> @event,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;

    /// <summary>
    /// Append event to aggregate with a existing or none existing stream.
    /// </summary>
    /// <param name="streamId">Id of our aggregate or stream.</param>
    /// <param name="event">domain event to append the aggregate.</param>
    /// <param name="expectedRevision"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEvent"></typeparam>
    /// <returns></returns>
    Task<AppendResult> AppendAsync<TEvent>(
        string streamId,
        IStreamEvent<TEvent> @event,
        ExpectedStreamVersion expectedRevision,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;

    /// <summary>
    /// Append events to aggregate with a existing or none existing stream.
    /// </summary>
    /// <param name="streamId">Id of our aggregate or stream.</param>
    /// <param name="events">domain event to append the aggregate.</param>
    /// <param name="expectedRevision"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEvent"></typeparam>
    /// <returns></returns>
    Task<AppendResult> AppendAsync<TEvent>(
        string streamId,
        IReadOnlyCollection<IStreamEvent<TEvent>> events,
        ExpectedStreamVersion expectedRevision,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;

    /// <summary>
    /// Rehydrating aggregate from events in the event store.
    /// </summary>
    /// <param name="streamId"></param>
    /// <param name="fromVersion"></param>
    /// <param name="getDefault"></param>
    /// <param name="when"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TAggregate"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <returns></returns>
    Task<TAggregate> AggregateStreamAsync<TAggregate, TId>(
        string streamId,
        StreamReadPosition fromVersion,
        Func<TAggregate> getDefault,
        Func<TAggregate, object, TAggregate> when,
        CancellationToken cancellationToken = default)
        where TAggregate : class, IEventSourcedAggregate<TId>;

    /// <summary>
    ///  Rehydrating aggregate from events in the event store.
    /// </summary>
    /// <param name="streamId"></param>
    /// <param name="getDefault"></param>
    /// <param name="when"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TAggregate"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <returns></returns>
    Task<TAggregate> AggregateStreamAsync<TAggregate, TId>(
        string streamId,
        Func<TAggregate> getDefault,
        Func<TAggregate, object, TAggregate> when,
        CancellationToken cancellationToken = default)
        where TAggregate : class, IEventSourcedAggregate<TId>;
}