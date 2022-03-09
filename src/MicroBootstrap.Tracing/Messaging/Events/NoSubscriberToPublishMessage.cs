using MicroBootstrap.Abstractions.Core.Domain.Events.External;

namespace MicroBootstrap.Tracing.Messaging.Events;

public class NoSubscriberToPublishMessage
{
    public NoSubscriberToPublishMessage(IIntegrationEvent eventData)
        => EventData = eventData;
    public IIntegrationEvent EventData { get; }
}
