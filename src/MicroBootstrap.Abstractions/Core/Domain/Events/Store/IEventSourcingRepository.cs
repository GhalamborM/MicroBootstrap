using MicroBootstrap.Abstractions.Core.Domain.Model.EventSourcing;

namespace MicroBootstrap.Abstractions.Core.Domain.Events.Store;

public interface IEventStoreRepository
{
    /// <summary>
    /// Load the aggregate from the store with a aggregate id
    /// </summary>
    /// <typeparam name="TAggregate">Type of aggregate.</typeparam>
    /// <typeparam name="TId">Type of Id.</typeparam>
    /// <param name="aggregateId">Id of aggregate.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task with aggregate as result.</returns>
    Task<TAggregate?> GetByIdAsync<TAggregate, TId>(
        TId aggregateId,
        CancellationToken cancellationToken = default)
        where TAggregate : IEventSourcedAggregate<TId>;

    /// <summary>
    /// Store an aggregate state to the store with using some events.
    /// </summary>
    /// <typeparam name="TAggregate">Type of aggregate.</typeparam>
    /// <typeparam name="TId">Type of Id.</typeparam>
    /// <param name="aggregate">Aggregate object to be saved.</param>
    /// <param name="expectedVersion">Expected version saved from earlier. -1 if new.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task of operation.</returns>
    Task Store<TAggregate, TId>(
        TAggregate aggregate,
        ExpectedStreamVersion? expectedVersion = null,
        CancellationToken cancellationToken = default)
        where TAggregate : IEventSourcedAggregate<TId>;

}