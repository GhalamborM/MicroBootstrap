using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Abstractions.Core.Domain.Model.EventSourcing;

namespace MicroBootstrap.Persistence.EventStoreDB.Repository;

public class EventStoreDbRepository<TAggregate, TId> : IEventSourcingRepository<TAggregate, TId>
    where TAggregate : IEventSourcedAggregate<TId>
{
    public Task<TAggregate?> GetByIdAsync(TId aggregateId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(
        TAggregate aggregate,
        long? expectedVersion = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Update(
        TAggregate aggregate,
        long? expectedRevision = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Delete(
        TAggregate aggregate,
        long? expectedRevision = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}