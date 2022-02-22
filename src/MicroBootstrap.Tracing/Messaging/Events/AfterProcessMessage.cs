﻿namespace MicroBootstrap.Tracing.Messaging.Events;

public class AfterProcessMessage
{
    public AfterProcessMessage(IIntegrationEvent eventData)
        => EventData = eventData;
    public IIntegrationEvent EventData { get; }
}
