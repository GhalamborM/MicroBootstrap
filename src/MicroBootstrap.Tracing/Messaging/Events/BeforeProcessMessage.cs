using MicroBootstrap.Abstractions.Core.Domain.Events.External;

namespace MicroBootstrap.Tracing.Messaging.Events;

public class BeforeProcessMessage
{
    public BeforeProcessMessage(IIntegrationEvent eventData)
        => EventData = eventData;
    public IIntegrationEvent EventData { get; }
}
