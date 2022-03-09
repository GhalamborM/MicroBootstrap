namespace MicroBootstrap.Abstractions.Core.Domain.Events.Store;

public record StreamEventMetadata(string EventId, long StreamPosition, long LogPosition) : IStreamEventMetadata;