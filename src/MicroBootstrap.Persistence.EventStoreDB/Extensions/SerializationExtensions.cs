using System.Text;
using EventStore.Client;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Core.Domain.Events;
using Newtonsoft.Json;

namespace MicroBootstrap.Persistence.EventStoreDB.Extensions;

public static class SerializationExtensions
{
    public static EventData ToJsonEventData(this IDomainEvent @event) =>
        new(
            Uuid.NewUuid(),
            EventTypeMapper.ToName(@event.GetType()),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { }))
        );
}