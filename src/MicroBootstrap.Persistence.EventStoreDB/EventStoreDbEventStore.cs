using MicroBootstrap.Abstractions.Core.Domain.Events;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;

namespace MicroBootstrap.Persistence.EventStoreDB;

public class EventStoreDbEventStore : IEventStore
{
    public Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IEvent>> GetAsync<TAggregateId>(
        TAggregateId aggregateId,
        int fromVersion,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}