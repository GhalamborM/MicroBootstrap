using System.Text;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using Newtonsoft.Json;

namespace MicroBootstrap.Core.Domain.Events.Store.InMemory;

public static class InMemorySerializationExtensions
{
    public static T DeserializeData<T>(this InMemoryStreamEvent resolvedEvent) => (T)DeserializeData(resolvedEvent);

    public static object DeserializeData(this InMemoryStreamEvent resolvedEvent)
    {
        // get type
        var eventType = TypeMapper.GetType(resolvedEvent.EventType);

        // deserialize event
        return JsonConvert.DeserializeObject(
            Encoding.UTF8.GetString(resolvedEvent.Data),
            eventType)!;
    }

    public static IStreamEventMetadata DeserializeMetadata(this InMemoryStreamEvent resolvedEvent)
    {
        // deserialize event
        return JsonConvert.DeserializeObject<StreamEventMetadata>(
            Encoding.UTF8.GetString(resolvedEvent.Metadata))!;
    }

    public static InMemoryStreamEvent ToJsonEventData(this IStreamEvent @event)
    {
        return ToJsonEventData(@event.Data, @event.Metadata);
    }

    public static InMemoryStreamEvent ToJsonEventData(this IDomainEvent @event, IStreamEventMetadata? metadata = null)
    {
        return new InMemoryStreamEvent
        {
            EventId = Guid.NewGuid(),
            EventType = TypeMapper.GetTypeNameByObject(@event),
            Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
            Metadata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata ?? new object())),
            ContentType = "application/json"
        };
    }

    public static StreamEvent ToStreamEvent(this InMemoryStreamEvent resolvedEvent)
    {
        var eventData = resolvedEvent.DeserializeData();
        var metaData = resolvedEvent.DeserializeMetadata();

        // var metaData = new StreamEventMetadata(
        //     resolvedEvent.Event.EventId.ToString(),
        //     resolvedEvent.Event.EventNumber.ToInt64());

        var type = typeof(StreamEvent<>).MakeGenericType(eventData.GetType());

        return (StreamEvent)Activator.CreateInstance(
            type,
            eventData,
            metaData)!;
    }
}