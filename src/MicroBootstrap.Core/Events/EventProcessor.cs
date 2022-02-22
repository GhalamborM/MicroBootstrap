namespace MicroBootstrap.Core.Events;

public class EventProcessor : IEventProcessor
{
    private readonly IMediator _mediator;
    private readonly IDomainEventPublisher _domainEventPublisher;
    private readonly IDomainNotificationEventPublisher _domainNotificationEventPublisher;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public EventProcessor(
        IMediator mediator,
        IDomainEventPublisher domainEventPublisher,
        IDomainNotificationEventPublisher domainNotificationEventPublisher,
        IIntegrationEventPublisher integrationEventPublisher)
    {
        _mediator = mediator;
        _domainEventPublisher = domainEventPublisher;
        _domainNotificationEventPublisher = domainNotificationEventPublisher;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        if (@event is IIntegrationEvent integrationEvent)
        {
            await _integrationEventPublisher.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
            return;
        }

        if (@event is IDomainEvent domainEvent)
        {
            await _domainEventPublisher.PublishAsync(domainEvent, cancellationToken: cancellationToken);
            return;
        }

        if (@event is IDomainNotificationEvent notificationEvent)
        {
            await _domainNotificationEventPublisher.PublishAsync(
                notificationEvent,
                cancellationToken: cancellationToken);
        }
    }

    public async Task PublishAsync<TEvent>(TEvent[] events, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        foreach (var @event in events)
        {
            await PublishAsync(@event, cancellationToken);
        }
    }

    public async Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        if (@event is IIntegrationEvent integrationEvent)
        {
            await _mediator.DispatchIntegrationEventAsync(integrationEvent, cancellationToken: cancellationToken);
            return;
        }

        if (@event is IDomainEvent domainEvent)
        {
            await _mediator.DispatchDomainEventAsync(domainEvent, cancellationToken: cancellationToken);
            return;
        }

        if (@event is IDomainNotificationEvent notificationEvent)
        {
            await _mediator.DispatchDomainNotificationEventAsync(
                notificationEvent,
                cancellationToken: cancellationToken);
            return;
        }

        await _mediator.Publish(@event, cancellationToken);
    }

    public async Task DispatchAsync<TEvent>(TEvent[] events, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        foreach (var @event in events)
        {
            await DispatchAsync(@event, cancellationToken);
        }
    }
}
