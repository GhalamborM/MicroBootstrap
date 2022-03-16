namespace MicroBootstrap.Core.Persistence.EventStore;

public class StreamEventData
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = null!;
    public byte[] Data { get; set; } = null!;
    public byte[]? Metadata { get; set; }
    public string EventType { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// Gets or sets the position of this event in the $all stream.
    /// </summary>
    public int EventPosition { get; set; }

    /// <summary>
    ///  Gets or sets the event number of this event in the stream.
    /// </summary>
    public int EventNumber { get; set; }
}