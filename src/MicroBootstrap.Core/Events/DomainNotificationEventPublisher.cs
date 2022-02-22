namespace MicroBootstrap.Core.Events;

public class DomainNotificationEventPublisher : IDomainNotificationEventPublisher
{
    private readonly IOutboxService _outboxService;

    public DomainNotificationEventPublisher(IOutboxService outboxService)
    {
        _outboxService = outboxService;
    }

    public Task PublishAsync(
        IDomainNotificationEvent domainNotificationEvent,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(domainNotificationEvent, nameof(domainNotificationEvent));

        return _outboxService.SaveAsync(domainNotificationEvent, cancellationToken);
    }

    public async Task PublishAsync(
        IDomainNotificationEvent[] domainNotificationEvents,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(domainNotificationEvents, nameof(domainNotificationEvents));

        foreach (var domainNotificationEvent in domainNotificationEvents)
        {
            await _outboxService.SaveAsync(domainNotificationEvent, cancellationToken);
        }
    }
}
