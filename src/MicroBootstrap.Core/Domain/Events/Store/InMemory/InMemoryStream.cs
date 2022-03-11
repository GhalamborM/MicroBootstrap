using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;

namespace MicroBootstrap.Core.Domain.Events.Store.InMemory;

record InMemoryEvent<TEvent>(IStreamEvent<TEvent> Event, int Position)
    where TEvent : IDomainEvent;

public class InMemoryStream
{
    private readonly List<dynamic> _events = new();

    public InMemoryStream(string name)
        => StreamName = name;

    public int Version { get; private set; } = -1;

    public string StreamName { get; }

    public void CheckVersion(ExpectedStreamVersion expectedVersion)
    {
        if (expectedVersion.Value != Version)
            throw new System.Exception($"Wrong stream version. Expected {expectedVersion.Value}, actual {Version}");
    }

    public void AppendEvents<TEvent>(
        ExpectedStreamVersion expectedVersion,
        IReadOnlyCollection<IStreamEvent<TEvent>> events)
        where TEvent : IDomainEvent
    {
        CheckVersion(expectedVersion);

        foreach (var streamEvent in events)
        {
            _events.Add(new InMemoryEvent<TEvent>(streamEvent, ++Version));
        }
    }

    public IEnumerable<IStreamEvent> GetEvents(StreamReadPosition from, long count)
    {
        var selected = _events
            .SkipWhile(x => x.Position < from.Value);

        if (count > 0) selected = selected.Take((int)count);

        return selected.Select(x => new StreamEvent(x.Data, x.Position, x.LogPosition, x.Metadata));
    }

    public IEnumerable<StreamEvent> GetEventsBackwards(int count)
    {
        var position = _events.Count - 1;

        while (count-- > 0)
        {
            yield return _events[position--].Event;
        }
    }
}