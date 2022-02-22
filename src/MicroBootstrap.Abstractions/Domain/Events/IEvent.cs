using MediatR;

namespace MicroBootstrap.Abstractions.Domain.Events;

/// <summary>
/// The event interface.
/// </summary>
public interface IEvent : INotification
{
    /// <summary>
    /// Gets the event identifier.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the event/aggregate root version.
    /// </summary>
    int EventVersion { get; }

    /// <summary>
    /// Gets the date the <see cref="Event"/> occurred on.
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// Gets type of this event.
    /// </summary>
    public string EventType { get; }
}
