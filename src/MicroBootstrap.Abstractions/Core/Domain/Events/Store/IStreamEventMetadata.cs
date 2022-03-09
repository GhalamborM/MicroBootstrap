namespace MicroBootstrap.Abstractions.Core.Domain.Events.Store;

public interface IStreamEventMetadata
{
    public string EventId { get; }
    public long StreamPosition { get; }
    public long LogPosition { get; }
}