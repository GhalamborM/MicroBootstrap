using EventStore.Client;
using MicroBootstrap.Core.Persistence.EventStore;

namespace MicroBootstrap.Persistence.EventStoreDB.Extensions;

public static class StreamEventExtensions
{
    public static IEnumerable<StreamEvent> ToStreamEvents(this IEnumerable<ResolvedEvent> resolvedEvents)
    {
        return resolvedEvents.Select(x => x.ToStreamEvent());
    }
}