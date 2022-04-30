using EventStore.Client;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Persistence.EventStore.Projections;
using MicroBootstrap.Core.Domain.Events;

namespace MicroBootstrap.Persistence.EventStoreDB.Extensions;

public static class EventStoreDBExtensions
{
    public static async Task<TEntity> Find<TEntity>(
        this EventStoreClient eventStore,
        Func<TEntity> getDefault,
        Func<TEntity, object, TEntity> when,
        string streamId,
        CancellationToken cancellationToken)
    {
        var readResult = eventStore.ReadStreamAsync(
            Direction.Forwards,
            streamId,
            StreamPosition.Start,
            cancellationToken: cancellationToken
        );

        return (await readResult
            .Select(@event => @event.DeserializeData()!)
            .AggregateAsync(
                getDefault(),
                when,
                cancellationToken
            ))!;
    }

    public static async Task<ulong> Append<TEvent>(
        this EventStoreClient eventStore,
        string streamId,
        TEvent @event,
        CancellationToken cancellationToken)
        where TEvent : IDomainEvent
    {
        var result = await eventStore.AppendToStreamAsync(
            streamId,
            StreamState.NoStream,
            new[] { @event.ToJsonEventData() },
            cancellationToken: cancellationToken
        );
        return result.NextExpectedStreamRevision;
    }


    public static async Task<ulong> Append<TEvent>(
        this EventStoreClient eventStore,
        string streamId,
        TEvent @event,
        ulong expectedRevision,
        CancellationToken cancellationToken)
        where TEvent : IDomainEvent
    {
        var result = await eventStore.AppendToStreamAsync(
            streamId,
            expectedRevision,
            new[] { @event.ToJsonEventData() },
            cancellationToken: cancellationToken
        );

        return result.NextExpectedStreamRevision;
    }

    public static async Task<T?> AggregateStream<T>(
        this EventStoreClient eventStore,
        Guid id,
        CancellationToken cancellationToken,
        ulong? fromVersion = null)
        where T : class, IHaveAggregateStateProjection
    {
        var readResult = eventStore.ReadStreamAsync(
            Direction.Forwards,
            StreamNameMapper.ToStreamId<T>(id),
            fromVersion ?? StreamPosition.Start,
            cancellationToken: cancellationToken
        );

        var aggregate = (T)Activator.CreateInstance(typeof(T), true)!;

        await foreach (var @event in readResult)
        {
            var eventData = @event.DeserializeData();

            aggregate.When(eventData!);
        }

        return aggregate;
    }
}