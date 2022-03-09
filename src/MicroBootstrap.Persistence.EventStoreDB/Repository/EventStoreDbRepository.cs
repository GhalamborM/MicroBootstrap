using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Abstractions.Core.Domain.Model;

namespace MicroBootstrap.Persistence.EventStoreDB.Repository;

public class EventStoreDbRepository : IEventSourcingRepository
{
    public Task<TAggregate> GetByIdAsync<TAggregate, TAggregateId>(
        TAggregateId aggregateId,
        CancellationToken cancellationToken = default)
        where TAggregate : IAggregate<TAggregateId>
    {
        throw new NotImplementedException();
    }

    public Task AddAsync<TAggregate>(
        TAggregate aggregate,
        long? expectedVersion = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}