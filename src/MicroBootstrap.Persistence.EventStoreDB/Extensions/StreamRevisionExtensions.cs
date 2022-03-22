using EventStore.Client;
using MicroBootstrap.Abstractions.Persistence.EventStore;

namespace MicroBootstrap.Persistence.EventStoreDB.Extensions;

public static class StreamRevisionExtensions
{
    public static StreamRevision AsStreamRevision(this ExpectedStreamVersion version)
        => StreamRevision.FromInt64(version.Value);

    public static StreamPosition AsStreamPosition(this StreamTruncatePosition position)
        => StreamPosition.FromInt64(position.Value);

    public static StreamPosition AsStreamPosition(this StreamReadPosition position)
        => StreamPosition.FromInt64(position.Value);
}