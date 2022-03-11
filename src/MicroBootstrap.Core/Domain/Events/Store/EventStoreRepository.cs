using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Abstractions.Core.Domain.Model.EventSourcing;
using MicroBootstrap.Abstractions.Domain.Exceptions;
using MicroBootstrap.Core.Domain.Events.Internal;
using MicroBootstrap.Core.Domain.Events.Store.Extensions;
using Nito.AsyncEx;

namespace MicroBootstrap.Core.Domain.Events.Store;

public class EventStoreRepository : IEventStoreRepository
{
    private readonly IEventStore _eventStore;

    public EventStoreRepository(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public Task<TAggregate?> GetByIdAsync<TAggregate, TId>(
        TId aggregateId,
        CancellationToken cancellationToken = default)
        where TAggregate : IEventSourcedAggregate<TId>
    {
        throw new NotImplementedException();
    }

    public async Task Store<TAggregate, TId>(
        TAggregate aggregate,
        ExpectedStreamVersion? expectedVersion = null,
        CancellationToken cancellationToken = default)
        where TAggregate : IEventSourcedAggregate<TId>
    {
        var streamName = StreamName.For<TAggregate, TId>(aggregate.Id);

        if (expectedVersion != null && (await _eventStore.GetAsync(
                streamName,
                new StreamReadPosition(expectedVersion.Value),
                cancellationToken).ConfigureAwait(false)).Any())
        {
            throw new ConcurrencyException<TId>(aggregate.Id);
        }
        else if (expectedVersion == null)
        {
            var savedChanges = await _eventStore.Get(aggregate.Id, aggregate.Version, cancellationToken)
                .ConfigureAwait(false);
            aggregate.LoadFromHistory(savedChanges);
        }

        ExpectedStreamVersion version = expectedVersion ?? new ExpectedStreamVersion(aggregate.Version);

        var events = aggregate.FlushUncommittedEvents();
        var initialVersion = aggregate.Version - events.Count;

        foreach (var domainEvent in events)
        {
            var streamEvent = domainEvent.ToStreamEvent();
            await _eventStore.AppendAsync(streamName, streamEvent, version, cancellationToken);
        }
    }
}