namespace MicroBootstrap.Abstractions.Core.Domain.Events.External;

/// <summary>
/// The integration event interface.
/// </summary>
public interface IIntegrationEvent : IEvent
{
    public string CorrelationId { get; }
}
