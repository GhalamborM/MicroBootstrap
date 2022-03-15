using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Core.Extensions.Utils.Reflections;

namespace MicroBootstrap.Core.Persistence.EventStore.Extensions;

public static class StreamEventExtensions
{
    public static IStreamEvent ToStreamEvent(
        this IDomainEvent domainEvent,
        IStreamEventMetadata? metadata)
    {
        return ReflectionHelpers.CreateGenericType(
            typeof(StreamEvent<>),
            new[] { domainEvent.GetType() },
            domainEvent,
            metadata);
    }
}