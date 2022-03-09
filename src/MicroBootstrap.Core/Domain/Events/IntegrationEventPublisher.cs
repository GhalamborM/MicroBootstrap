using Ardalis.GuardClauses;
using MicroBootstrap.Abstractions.Core.Domain.Events.External;
using MicroBootstrap.Abstractions.Messaging.Outbox;

namespace MicroBootstrap.Core.Domain.Events;

public class IntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IOutboxService _outboxService;

    public IntegrationEventPublisher(IOutboxService outboxService)
    {
        _outboxService = outboxService;
    }

    public Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(integrationEvent, nameof(integrationEvent));

        return _outboxService.SaveAsync(integrationEvent, cancellationToken);
    }

    public async Task PublishAsync(IIntegrationEvent[] integrationEvents, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(integrationEvents, nameof(integrationEvents));

        foreach (var integrationEvent in integrationEvents)
        {
            await PublishAsync(integrationEvent, cancellationToken);
        }
    }
}
