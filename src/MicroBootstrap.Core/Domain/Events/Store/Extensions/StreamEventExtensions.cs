using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;

namespace MicroBootstrap.Core.Domain.Events.Store.Extensions;

public static class StreamEventExtensions
{
    public static dynamic ToStreamEvent(
        this IDomainEvent domainEvent,
        IStreamEventMetadata? metadata = null)
    {
        var type = typeof(StreamEvent<>).MakeGenericType(domainEvent.GetType());
        return (StreamEvent)Activator.CreateInstance(type, domainEvent, metadata)!;
    }
}