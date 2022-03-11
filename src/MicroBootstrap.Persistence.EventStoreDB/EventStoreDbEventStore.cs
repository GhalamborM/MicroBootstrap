using EventStore.Client;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Abstractions.Core.Domain.Model.EventSourcing;
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

    public async Task<IEnumerable<IStreamEvent>> GetAsync(
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

    public Task<IEnumerable<IStreamEvent>> GetAsync(
        string streamId,
        StreamReadPosition? fromVersion = null,
        CancellationToken cancellationToken = default)
    {
        return GetAsync(streamId, fromVersion, long.MaxValue, cancellationToken);
    }

    public async Task<AppendResult> AppendAsync<TEvent>(
        string streamId,
        IStreamEvent<TEvent> @event,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        EventData eventData = @event.ToJsonEventData();

        var result = await _grpcClient.AppendToStreamAsync(
            streamId,
            StreamState.NoStream,
            new List<EventData> { eventData },
            cancellationToken: cancellationToken);

        return new AppendResult((long)result.LogPosition.CommitPosition, result.NextExpectedStreamRevision.ToInt64());
    }

    public async Task<AppendResult> AppendAsync<TEvent>(
        string streamId,
        IStreamEvent<TEvent> @event,
        ExpectedStreamVersion expectedRevision,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
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

    public async Task<AppendResult> AppendAsync<TEvent>(
        string streamId,
        IReadOnlyCollection<IStreamEvent<TEvent>> events,
        ExpectedStreamVersion expectedRevision,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
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
        Func<TAggregate> getDefault,
        Func<TAggregate, object, TAggregate> when,
        CancellationToken cancellationToken = default)
        where TAggregate : class, IEventSourcedAggregate<TId>
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
                getDefault.Invoke(),
                when,
                cancellationToken
            );
    }

    public Task<TAggregate> AggregateStreamAsync<TAggregate, TId>(
        string streamId,
        Func<TAggregate> getDefault,
        Func<TAggregate, object, TAggregate> when,
        CancellationToken cancellationToken = default)
        where TAggregate : class, IEventSourcedAggregate<TId>
    {
        return AggregateStreamAsync<TAggregate, TId>(
            streamId,
            StreamReadPosition.Start,
            getDefault,
            when,
            cancellationToken);
    }
}