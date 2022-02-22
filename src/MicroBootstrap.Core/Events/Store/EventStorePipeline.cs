namespace MicroBootstrap.Core.Events.Store;

public class EventStorePipeline<TEvent> : IEventHandler<TEvent>
    where TEvent : IEvent
{
    private readonly IEventStore _eventStore;

    public EventStorePipeline(IEventStore eventStore)
    {
        _eventStore = Guard.Against.Null(eventStore, nameof(eventStore));
    }

    public async Task Handle(TEvent @event, CancellationToken cancellationToken)
    {
        await _eventStore.Append(@event.EventId, cancellationToken, @event);
        await _eventStore.SaveChanges(cancellationToken);
    }
}
