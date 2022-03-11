namespace MicroBootstrap.Abstractions.Core.Domain.Events.Store;

public interface IStreamEventMetadata
{
    string EventId { get; }
    long? LogPosition { get; }
    long StreamPosition { get; }
}