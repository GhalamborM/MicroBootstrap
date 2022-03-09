using MicroBootstrap.Abstractions.Core.Domain.Events.External;

namespace MicroBootstrap.Core.Domain.Events.External;

public abstract record IntegrationEvent : Event, IIntegrationEvent
{
    public string CorrelationId { get; protected set; } = default;
}
