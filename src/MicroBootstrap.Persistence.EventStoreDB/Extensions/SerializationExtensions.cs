using System.Text;
using Core.Serialization.Newtonsoft;
using EventStore.Client;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Core.Domain.Events;
using MicroBootstrap.Core.Domain.Events.Store;
using Newtonsoft.Json;

namespace MicroBootstrap.Persistence.EventStoreDB.Extensions;

public static class SerializationExtensions
{
    private static readonly JsonSerializerSettings _serializerSettings =
        new JsonSerializerSettings().WithNonDefaultConstructorContractResolver();


    public static T Deserialize<T>(this ResolvedEvent resolvedEvent) => (T)Deserialize(resolvedEvent);

    public static object Deserialize(this ResolvedEvent resolvedEvent)
    {
        // get type
        var eventType = EventTypeMapper.ToType(resolvedEvent.Event.EventType);

        // deserialize event
        return JsonConvert.DeserializeObject(
            Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span),
            eventType,
            _serializerSettings)!;
    }

    public static EventData ToJsonEventData<T>(this IStreamEvent<T> @event)
        where T : IDomainEvent
        => new(
            Uuid.NewUuid(),
            EventTypeMapper.ToName(@event.GetType()),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event.Data, _serializerSettings)),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event.Metadata ?? new object(), _serializerSettings))
        );

    public static IEnumerable<EventData> ToJsonEventData<T>(this IEnumerable<IStreamEvent<T>> events)
        where T : IDomainEvent
        => events.Select(e => e.ToJsonEventData());

    public static EventData ToJsonEventData<T>(this T @event, IStreamEventMetadata? metadata = null)
        where T : IDomainEvent
        => new(
            Uuid.NewUuid(),
            EventTypeMapper.ToName(@event.GetType()),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event, _serializerSettings)),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata ?? new object(), _serializerSettings))
        );

    public static StreamEvent ToStreamEvent(this ResolvedEvent resolvedEvent)
    {
        var eventData = resolvedEvent.Deserialize();

        var metaData = new StreamEventMetadata(resolvedEvent.Event.EventId.ToString());

        var type = typeof(StreamEvent<>).MakeGenericType(eventData.GetType());
        return (StreamEvent)Activator.CreateInstance(
            type,
            eventData,
            resolvedEvent.Event.EventNumber.ToInt64(),
            (long)resolvedEvent.Event.Position.CommitPosition,
            metaData)!;
    }
}