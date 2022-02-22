namespace MicroBootstrap.Abstractions.Domain.Events.External;

public abstract record IntegrationEvent : Event, IIntegrationEvent
{
    public string CorrelationId { get; protected set; } = default;
}
