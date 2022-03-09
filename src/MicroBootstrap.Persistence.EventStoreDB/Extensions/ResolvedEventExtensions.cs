using System.Text;
using Core.Serialization.Newtonsoft;
using EventStore.Client;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Core.Domain.Events;
using Newtonsoft.Json;

namespace MicroBootstrap.Persistence.EventStoreDB.Extensions;

public static class ResolvedEventExtensions
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

    public static StreamEvent? ToStreamEvent(this ResolvedEvent resolvedEvent)
    {
        var eventData = resolvedEvent.Deserialize();

        if (eventData == null)
            return null;

        var metaData = new StreamEventMetadata(
            resolvedEvent.Event.EventId.ToString(),
            resolvedEvent.Event.EventNumber.ToInt64(),
            (long)resolvedEvent.Event.Position.CommitPosition
        );
        var type = typeof(StreamEvent<>).MakeGenericType(eventData.GetType());
        return (StreamEvent)Activator.CreateInstance(type, eventData, metaData)!;
    }
}