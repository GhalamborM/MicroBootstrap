using MicroBootstrap.Abstractions.Core.Domain.Events.Store;

namespace MicroBootstrap.Core.Domain.Events.Store;

public record StreamEventMetadata(string EventId, long StreamPosition) : IStreamEventMetadata
{
    public long? LogPosition { get; }
}