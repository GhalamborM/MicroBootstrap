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
    private static readonly JsonSerializerSettings SerializerSettings =
        new JsonSerializerSettings().WithNonDefaultConstructorContractResolver();


    public static T DeserializeData<T>(this ResolvedEvent resolvedEvent) => (T)DeserializeData(resolvedEvent);

    public static object DeserializeData(this ResolvedEvent resolvedEvent)
    {
        // get type
        var eventType = TypeMapper.GetType(resolvedEvent.Event.EventType);

        // deserialize event
        return JsonConvert.DeserializeObject(
            Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span),
            eventType,
            SerializerSettings)!;
    }

    public static IStreamEventMetadata DeserializeMetadata(this ResolvedEvent resolvedEvent)
    {
        // deserialize event
        return JsonConvert.DeserializeObject<StreamEventMetadata>(
            Encoding.UTF8.GetString(resolvedEvent.Event.Metadata.Span),
            SerializerSettings
        )!;
    }

    public static EventData ToJsonEventData(this IStreamEvent @event)
    {
        return ToJsonEventData(@event.Data, @event.Metadata);
    }

    public static EventData ToJsonEventData(this IDomainEvent @event, IStreamEventMetadata? metadata = null)
    {
        return new(
            Uuid.NewUuid(),
            TypeMapper.GetTypeNameByObject(@event),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event, SerializerSettings)),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata ?? new object(), SerializerSettings))
        );
    }

    public static StreamEvent ToStreamEvent(this ResolvedEvent resolvedEvent)
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