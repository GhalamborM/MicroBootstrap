using System.Collections.Immutable;
using Ardalis.GuardClauses;
using MicroBootstrap.Abstractions.Core.Domain.Events;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Abstractions.Core.Domain.Model.EventSourcing;
using MicroBootstrap.Core.Domain.Events.Store.Extensions;

namespace MicroBootstrap.Core.Domain.Events.Store;

public class EventStoreRepository : IEventStoreRepository
{
    private readonly IEventStore _eventStore;
    private readonly IDomainEventPublisher _domainEventPublisher;

    public EventStoreRepository(IEventStore eventStore, IDomainEventPublisher domainEventPublisher)
    {
        _eventStore = eventStore;
        _domainEventPublisher = domainEventPublisher;
    }

    public async Task<TAggregate?> GetAsync<TAggregate, TId>(
        TId aggregateId,
        CancellationToken cancellationToken = default)
        where TAggregate : IEventSourcedAggregate<TId>, new()
    {
        Guard.Against.Null(aggregateId, nameof(aggregateId));

        var streamName = StreamName.For<TAggregate, TId>(aggregateId);

        var defaultAggregateState = AggregateFactory<TAggregate>.CreateAggregate();

        var result = await _eventStore.AggregateStreamAsync<TAggregate, TId>(
            streamName,
            StreamReadPosition.Start,
            defaultAggregateState,
            defaultAggregateState.Fold,
            cancellationToken);

        return result;
    }

    public async Task<AppendResult> Store<TAggregate, TId>(
        TAggregate aggregate,
        ExpectedStreamVersion? expectedVersion = null,
        CancellationToken cancellationToken = default)
        where TAggregate : IEventSourcedAggregate<TId>
    {
        Guard.Against.Null(aggregate, nameof(aggregate));

        var streamName = StreamName.For<TAggregate, TId>(aggregate.Id);

        ExpectedStreamVersion version = expectedVersion ?? new ExpectedStreamVersion(aggregate.OriginalVersion);

        var events = aggregate.FlushUncommittedEvents();

        var result = await _eventStore.AppendEventsAsync(
            streamName,
            events.Select(
                    x => x.ToStreamEvent(new StreamEventMetadata(x.EventId.ToString(), x.AggregateSequenceNumber)))
                .ToImmutableList(),
            version,
            cancellationToken);

        await _domainEventPublisher.PublishAsync(events.ToArray(), cancellationToken);

        // TODO: Handling Projections and Snapshots

        return result;
    }

    public Task Store<TAggregate, TId>(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
        where TAggregate : IEventSourcedAggregate<TId>
    {
        return Store<TAggregate, TId>(
            aggregate,
            new ExpectedStreamVersion(aggregate.OriginalVersion),
            cancellationToken);
    }

    public Task<bool> Exists<TAggregate, TId>(TId aggregateId, CancellationToken cancellationToken = default)
        where TAggregate : IEventSourcedAggregate<TId>
    {
        Guard.Against.Null(aggregateId, nameof(aggregateId));

        var streamName = StreamName.For<TAggregate, TId>(aggregateId);

        return _eventStore.StreamExists(streamName, cancellationToken);
    }
}