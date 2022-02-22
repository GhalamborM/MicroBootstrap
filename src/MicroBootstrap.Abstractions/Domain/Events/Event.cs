namespace MicroBootstrap.Abstractions.Domain.Events;

/// <inheritdoc cref="IEvent"/>
public abstract record Event : IEvent
{
    public Guid EventId { get; protected set; } = Guid.NewGuid();

    public int EventVersion { get; protected set; } = 1;

    public DateTime OccurredOn { get; protected set; } = DateTime.Now;

    public string EventType { get { return GetType().AssemblyQualifiedName; } }
}
