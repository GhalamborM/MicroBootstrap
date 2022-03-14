namespace MicroBootstrap.Core.Domain.Events.Store.InMemory;

public class InMemoryStreamEvent
{
    public Guid EventId { get; set; }
    public string Name { get; set; }
    public byte[] Data { get; set; }
    public byte[]? Metadata { get; set; }
    public string EventType { get; set; }
    public string ContentType { get; set; }
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// The position of this event in the $all stream.
    /// </summary>
    public int EventPosition { get; set; }

    /// <summary>
    ///  The event number of this event in the stream.
    /// </summary>
    public int EventNumber { get; set; }
}