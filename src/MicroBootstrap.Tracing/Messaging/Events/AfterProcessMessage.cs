using MicroBootstrap.Abstractions.Core.Domain.Events.External;

namespace MicroBootstrap.Tracing.Messaging.Events;

public class AfterProcessMessage
{
    public AfterProcessMessage(IIntegrationEvent eventData)
        => EventData = eventData;
    public IIntegrationEvent EventData { get; }
}
