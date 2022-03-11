using EventStore.Client;
using MicroBootstrap.Core.Domain.Events.Store;

namespace MicroBootstrap.Persistence.EventStoreDB.Extensions;

public static class StreamEventExtensions
{
    public static IEnumerable<StreamEvent> ToStreamEvents(this IEnumerable<ResolvedEvent> resolvedEvents)
    {
        return resolvedEvents.Select(x => x.ToStreamEvent());
    }
}