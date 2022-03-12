using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Abstractions.Core.Domain.Model.EventSourcing;

namespace MicroBootstrap.Core.Domain.Events.Store.InMemory;

public class InMemoryEventStore : IEventStore
{
    private readonly Dictionary<string, InMemoryStream> _storage = new();
    private readonly List<dynamic> _global = new();

    public Task<IEnumerable<IStreamEvent>> GetStreamEventsAsync(
        string streamId,
        StreamReadPosition? fromVersion = null,
        long maxCount = long.MaxValue,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(FindStream(streamId).GetEvents(fromVersion ?? StreamReadPosition.Start, maxCount));
    }

    public Task<IEnumerable<IStreamEvent>> GetStreamEventsAsync(
        string streamId,
        StreamReadPosition? fromVersion = null,
        CancellationToken cancellationToken = default)
    {
        return GetStreamEventsAsync(streamId, fromVersion, long.MaxValue, cancellationToken);
    }

    public Task<AppendResult> AppendEventAsync(
        string streamId,
        IStreamEvent @event,
        CancellationToken cancellationToken = default)
    {
        if (!_storage.TryGetValue(streamId, out var existing))
        {
            existing = new InMemoryStream(streamId);
        }

        var events = new List<IStreamEvent> { @event };
        existing.AppendEvents(ExpectedStreamVersion.NoStream, events.AsReadOnly());

        _global.AddRange(events);

        return Task.FromResult(
            new AppendResult(_global.Count - 1, existing.Version)
        );
    }

    public Task<AppendResult> AppendEventAsync(
        string streamId,
        IStreamEvent @event,
        ExpectedStreamVersion expectedRevision,
        CancellationToken cancellationToken = default)
    {
        if (!_storage.TryGetValue(streamId, out var existing))
        {
            existing = new InMemoryStream(streamId);
        }

        var events = new List<IStreamEvent> { @event };
        existing.AppendEvents(expectedRevision, events.AsReadOnly());

        _global.AddRange(events);

        return Task.FromResult(
            new AppendResult(_global.Count - 1, existing.Version)
        );
    }

    public Task<AppendResult> AppendEventsAsync(
        string streamId,
        IReadOnlyCollection<IStreamEvent> events,
        ExpectedStreamVersion expectedRevision,
        CancellationToken cancellationToken = default)
    {
        if (!_storage.TryGetValue(streamId, out var existing))
        {
            existing = new InMemoryStream(streamId);
        }

        existing.AppendEvents(expectedRevision, events);

        _global.AddRange(events);

        return Task.FromResult(
            new AppendResult(_global.Count - 1, existing.Version)
        );
    }

    public Task<TAggregate> AggregateStreamAsync<TAggregate, TId>(
        string streamId,
        StreamReadPosition fromVersion,
        Func<TAggregate> defaultAggregate,
        Action<object> fold,
        CancellationToken cancellationToken = default)
        where TAggregate : IEventSourcedAggregate<TId>, new()
    {
        var streamEvents = FindStream(streamId).GetEvents(fromVersion, long.MaxValue).ToList();
        var aggregate = AggregateFactory<TAggregate>.CreateAggregate();

        var result = streamEvents.Aggregate(
            aggregate,
            (agg, @event) =>
            {
                fold(@event);
                return agg;
            }
        );

        return Task.FromResult(result);
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

    private InMemoryStream FindStream(string stream)
    {
        if (!_storage.TryGetValue(stream, out var existing))
            throw new System.Exception($"Stream with name: {stream} not found.");

        return existing;
    }
}