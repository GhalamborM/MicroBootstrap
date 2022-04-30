namespace MicroBootstrap.Abstractions.Messaging.Outbox;

[Flags]
public enum EventType
{
    IntegrationEvent = 1,
    DomainNotificationEvent = 2
}
