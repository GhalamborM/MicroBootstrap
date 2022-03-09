using MicroBootstrap.Abstractions.Core.Domain.Model;

namespace MicroBootstrap.Abstractions.Core.Domain.Events.Store;

public interface IEventSourcingRepository
{
    /// <summary>
    /// Fetches aggregate
    /// </summary>
    /// <typeparam name="TAggregate">Type of aggregate.</typeparam>
    /// <typeparam name="TAggregateId">Type of aggregate id.</typeparam>
    /// <param name="aggregateId">Id of aggregate.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task with aggregate as result.</returns>
    Task<TAggregate> GetByIdAsync<TAggregate, TAggregateId>(
        TAggregateId aggregateId,
        CancellationToken cancellationToken = default)
        where TAggregate : IAggregate<TAggregateId>;

    /// <summary>
    /// Add our aggregate as event sourced in event-store.
    /// </summary>
    /// <typeparam name="TAggregate">Type of aggregate.</typeparam>
    /// <param name="aggregate">Aggregate object to be saved.</param>
    /// <param name="expectedVersion">Expected version saved from earlier. -1 if new.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task of operation.</returns>
    Task AddAsync<TAggregate>(
        TAggregate aggregate,
        long? expectedVersion = null,
        CancellationToken cancellationToken = default);
}