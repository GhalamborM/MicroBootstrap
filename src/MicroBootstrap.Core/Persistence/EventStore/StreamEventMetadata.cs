using MicroBootstrap.Abstractions.Persistence.EventStore;

namespace MicroBootstrap.Core.Persistence.EventStore;

public record StreamEventMetadata(string EventId, long StreamPosition) : IStreamEventMetadata
{
    public long? LogPosition { get; }
}