using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Abstractions.Core.Domain.Model.EventSourcing;

namespace MicroBootstrap.Core.Domain.Events.Store.InMemory;

public class InMemoryEventStore : IEventStore
{
    private readonly Dictionary<string, InMemoryStream> _storage = new();
    private readonly List<dynamic> _global = new();

    public Task<IEnumerable<IStreamEvent>> GetAsync(
        string streamId,
        StreamReadPosition? fromVersion = null,
        long maxCount = long.MaxValue,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(FindStream(streamId).GetEvents(fromVersion ?? StreamReadPosition.Start, maxCount));
    }

    public Task<IEnumerable<IStreamEvent>> GetAsync(
        string streamId,
        StreamReadPosition? fromVersion = null,
        CancellationToken cancellationToken = default)
    {
        return GetAsync(streamId, fromVersion, long.MaxValue, cancellationToken);
    }

    public Task<AppendResult> AppendAsync<TEvent>(
        string streamId,
        IStreamEvent<TEvent> @event,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        if (!_storage.TryGetValue(streamId, out var existing))
        {
            existing = new InMemoryStream(streamId);
        }

        var events = new List<IStreamEvent<TEvent>> { @event };
        existing.AppendEvents(ExpectedStreamVersion.NoStream, events.AsReadOnly());

        _global.AddRange(events);

        return Task.FromResult(
            new AppendResult(_global.Count - 1, existing.Version)
        );
    }

    public Task<AppendResult> AppendAsync<TEvent>(
        string streamId,
        IStreamEvent<TEvent> @event,
        ExpectedStreamVersion expectedRevision,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        if (!_storage.TryGetValue(streamId, out var existing))
        {
            existing = new InMemoryStream(streamId);
        }

        var events = new List<IStreamEvent<TEvent>> { @event };
        existing.AppendEvents(expectedRevision, events.AsReadOnly());

        _global.AddRange(events);

        return Task.FromResult(
            new AppendResult(_global.Count - 1, existing.Version)
        );
    }

    public Task<AppendResult> AppendAsync<TEvent>(
        string streamId,
        IReadOnlyCollection<IStreamEvent<TEvent>> events,
        ExpectedStreamVersion expectedRevision,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
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
        Func<TAggregate> getDefault,
        Func<TAggregate, object, TAggregate> when,
        CancellationToken cancellationToken = default)
        where TAggregate : class, IEventSourcedAggregate<TId>
    {
        var streamEvents = FindStream(streamId).GetEvents(fromVersion, long.MaxValue).ToList();

        var result = streamEvents.Aggregate(
            getDefault.Invoke(),
            when
        );

        return Task.FromResult(result);
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

    private InMemoryStream FindStream(string stream)
    {
        if (!_storage.TryGetValue(stream, out var existing))
            throw new System.Exception($"Stream with name: {stream} not found.");

        return existing;
    }
}