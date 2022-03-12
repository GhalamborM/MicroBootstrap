using EventStore.Client;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Abstractions.Core.Domain.Model.EventSourcing;
using MicroBootstrap.Core.Domain;
using MicroBootstrap.Persistence.EventStoreDB.Extensions;

namespace MicroBootstrap.Persistence.EventStoreDB;

// https://developers.eventstore.com/clients/dotnet/21.2/migration-to-gRPC.html
public class EventStoreDbEventStore : IEventStore
{
    private readonly EventStoreClient _grpcClient;

    public EventStoreDbEventStore(EventStoreClient grpcClient)
    {
        _grpcClient = grpcClient;
    }

    public async Task<IEnumerable<IStreamEvent>> GetStreamEventsAsync(
        string streamId,
        StreamReadPosition? fromVersion = null,
        long maxCount = long.MaxValue,
        CancellationToken cancellationToken = default)
    {
        var readResult = _grpcClient.ReadStreamAsync(
            Direction.Forwards,
            streamId,
            fromVersion?.AsStreamPosition() ?? StreamPosition.Start,
            maxCount,
            cancellationToken: cancellationToken
        );

        var resolvedEvents = await readResult.ToListAsync(cancellationToken);

        return resolvedEvents.ToStreamEvents();
    }

    public Task<IEnumerable<IStreamEvent>> GetStreamEventsAsync(
        string streamId,
        StreamReadPosition? fromVersion = null,
        CancellationToken cancellationToken = default)
    {
        return GetStreamEventsAsync(streamId, fromVersion, long.MaxValue, cancellationToken);
    }

    public async Task<AppendResult> AppendEventAsync(
        string streamId,
        IStreamEvent @event,
        CancellationToken cancellationToken = default)
    {
        EventData eventData = @event.ToJsonEventData();

        var result = await _grpcClient.AppendToStreamAsync(
            streamId,
            StreamState.NoStream,
            new List<EventData> { eventData },
            cancellationToken: cancellationToken);

        return new AppendResult((long)result.LogPosition.CommitPosition, result.NextExpectedStreamRevision.ToInt64());
    }

    public async Task<AppendResult> AppendEventAsync(
        string streamId,
        IStreamEvent @event,
        ExpectedStreamVersion expectedRevision,
        CancellationToken cancellationToken = default)
    {
        EventData eventData = @event.ToJsonEventData();

        if (expectedRevision == ExpectedStreamVersion.NoStream)
        {
            var result = await _grpcClient.AppendToStreamAsync(
                streamId,
                StreamState.NoStream,
                new List<EventData> { eventData },
                cancellationToken: cancellationToken);

            return new AppendResult(
                (long)result.LogPosition.CommitPosition,
                result.NextExpectedStreamRevision.ToInt64());
        }

        if (expectedRevision == ExpectedStreamVersion.Any)
        {
            var result = await _grpcClient.AppendToStreamAsync(
                streamId,
                StreamState.Any,
                new List<EventData> { eventData },
                cancellationToken: cancellationToken);

            return new AppendResult(
                (long)result.LogPosition.CommitPosition,
                result.NextExpectedStreamRevision.ToInt64());
        }
        else
        {
            var result = await _grpcClient.AppendToStreamAsync(
                streamId,
                expectedRevision.AsStreamRevision(),
                new List<EventData> { eventData },
                cancellationToken: cancellationToken);

            return new AppendResult(
                (long)result.LogPosition.CommitPosition,
                result.NextExpectedStreamRevision.ToInt64());
        }
    }

    public async Task<AppendResult> AppendEventsAsync(
        string streamId,
        IReadOnlyCollection<IStreamEvent> events,
        ExpectedStreamVersion expectedRevision,
        CancellationToken cancellationToken = default)
    {
        var eventsData = events.ToJsonEventData();

        if (expectedRevision == ExpectedStreamVersion.NoStream)
        {
            var result = await _grpcClient.AppendToStreamAsync(
                streamId,
                StreamState.NoStream,
                eventsData,
                cancellationToken: cancellationToken);

            return new AppendResult(
                (long)result.LogPosition.CommitPosition,
                result.NextExpectedStreamRevision.ToInt64());
        }

        if (expectedRevision == ExpectedStreamVersion.Any)
        {
            var result = await _grpcClient.AppendToStreamAsync(
                streamId,
                StreamState.Any,
                eventsData,
                cancellationToken: cancellationToken);

            return new AppendResult(
                (long)result.LogPosition.CommitPosition,
                result.NextExpectedStreamRevision.ToInt64());
        }
        else
        {
            var result = await _grpcClient.AppendToStreamAsync(
                streamId,
                expectedRevision.AsStreamRevision(),
                eventsData,
                cancellationToken: cancellationToken);

            return new AppendResult(
                (long)result.LogPosition.CommitPosition,
                result.NextExpectedStreamRevision.ToInt64());
        }
    }

    public async Task<TAggregate> AggregateStreamAsync<TAggregate, TId>(
        string streamId,
        StreamReadPosition fromVersion,
        Func<TAggregate> defaultAggregate,
        Action<object> fold,
        CancellationToken cancellationToken = default)
        where TAggregate : IEventSourcedAggregate<TId>, new()
    {
        var readResult = _grpcClient.ReadStreamAsync(
            Direction.Forwards,
            streamId,
            fromVersion.AsStreamPosition(),
            cancellationToken: cancellationToken
        );

        return await readResult
            .Select(@event => @event.Deserialize()!)
            .AggregateAsync(
                defaultAggregate(),
                (agg, @event) =>
                {
                    fold(@event);
                    return agg;
                },
                cancellationToken
            );
    }

    public Task<TAggregate> AggregateStreamAsync<TAggregate, TId>(
        string streamId,
        Func<TAggregate> defaultAggregate,
        Action<object> fold,
        CancellationToken cancellationToken = default)
        where TAggregate : IEventSourcedAggregate<TId>, new()
    {
        return AggregateStreamAsync<TAggregate, TId>(
            streamId,
            StreamReadPosition.Start,
            defaultAggregate,
            fold,
            cancellationToken);
    }
}