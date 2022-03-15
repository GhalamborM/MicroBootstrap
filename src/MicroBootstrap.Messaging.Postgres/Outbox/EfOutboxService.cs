using Ardalis.GuardClauses;
using Humanizer;
using MediatR;
using MicroBootstrap.Abstractions.Core.Domain.Events.External;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Messaging.Outbox;
using MicroBootstrap.Abstractions.Messaging.Serialization;
using MicroBootstrap.Abstractions.Messaging.Transport;
using MicroBootstrap.Core.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MicroBootstrap.Messaging.Outbox;

public class EfOutboxService<TContext> : IOutboxService
    where TContext : EfDbContextBase
{
    private readonly OutboxOptions _options;
    private readonly ILogger<EfOutboxService<TContext>> _logger;
    private readonly IMessageSerializer _messageSerializer;
    private readonly IEventBusPublisher _eventBusPublisher;
    private readonly IMediator _mediator;
    private readonly OutboxDataContext _outboxDataContext;

    public EfOutboxService(
        IOptions<OutboxOptions> options,
        ILogger<EfOutboxService<TContext>> logger,
        IMessageSerializer messageSerializer,
        IEventBusPublisher eventBusPublisher,
        IMediator mediator,
        OutboxDataContext outboxDataContext)
    {
        _options = options.Value;
        _logger = logger;
        _messageSerializer = messageSerializer;
        _eventBusPublisher = eventBusPublisher;
        _mediator = mediator;
        _outboxDataContext = outboxDataContext;
    }

    public async Task<IEnumerable<OutboxMessage>> GetAllUnsentOutboxMessagesAsync(
        EventType eventType = EventType.IntegrationEvent | EventType.DomainNotificationEvent,
        CancellationToken cancellationToken = default)
    {
        var messages = await _outboxDataContext.OutboxMessages
            .Where(x => x.EventType == eventType && x.ProcessedOn == null)
            .ToListAsync(cancellationToken: cancellationToken);

        return messages;
    }

    public async Task<IEnumerable<OutboxMessage>> GetAllOutboxMessagesAsync(
        EventType eventType = EventType.IntegrationEvent | EventType.DomainNotificationEvent,
        CancellationToken cancellationToken = default)
    {
        var messages = await _outboxDataContext.OutboxMessages
            .Where(x => x.EventType == eventType).ToListAsync(cancellationToken: cancellationToken);

        return messages;
    }

    public async Task CleanProcessedAsync(CancellationToken cancellationToken = default)
    {
        var messages = await _outboxDataContext.OutboxMessages
            .Where(x => x.ProcessedOn != null).ToListAsync(cancellationToken: cancellationToken);

        _outboxDataContext.OutboxMessages.RemoveRange(messages);
        await _outboxDataContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(integrationEvent, nameof(integrationEvent));

        await SaveAsync(new[] { integrationEvent }, cancellationToken);
    }

    public async Task SaveAsync(IIntegrationEvent[] integrationEvents, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(integrationEvents, nameof(integrationEvents));

        if (integrationEvents.Any() == false)
            return;

        if (!_options.Enabled)
        {
            _logger.LogWarning("Outbox is disabled, outgoing messages won't be saved");
            return;
        }

        foreach (var integrationEvent in integrationEvents)
        {
            string name = integrationEvent.GetType().Name;

            var outboxMessages = new OutboxMessage(
                integrationEvent.EventId,
                integrationEvent.OccurredOn,
                integrationEvent.EventType,
                name.Underscore(),
                _messageSerializer.Serialize(integrationEvent),
                EventType.IntegrationEvent,
                correlationId: null);

            await _outboxDataContext.OutboxMessages.AddAsync(outboxMessages, cancellationToken);
        }

        await _outboxDataContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Saved message to the outbox");
    }

    public async Task SaveAsync(
        IDomainNotificationEvent domainNotificationEvent,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(domainNotificationEvent, nameof(domainNotificationEvent));

        await SaveAsync(new[] { domainNotificationEvent }, cancellationToken);
    }

    public async Task SaveAsync(
        IDomainNotificationEvent[] domainNotificationEvents,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(domainNotificationEvents, nameof(domainNotificationEvents));

        if (domainNotificationEvents.Any() == false)
            return;

        if (!_options.Enabled)
        {
            _logger.LogWarning("Outbox is disabled, outgoing messages won't be saved");
            return;
        }

        foreach (var domainNotificationEvent in domainNotificationEvents)
        {
            string name = domainNotificationEvent.GetType().Name;

            var outboxMessages = new OutboxMessage(
                domainNotificationEvent.EventId,
                domainNotificationEvent.OccurredOn,
                domainNotificationEvent.EventType,
                name.Underscore(),
                _messageSerializer.Serialize(domainNotificationEvent),
                EventType.DomainNotificationEvent,
                correlationId: null);

            await _outboxDataContext.OutboxMessages.AddAsync(outboxMessages, cancellationToken);
        }

        await _outboxDataContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Saved message to the outbox");
    }

    public async Task PublishUnsentOutboxMessagesAsync(CancellationToken cancellationToken = default)
    {
        var unsentMessages = await _outboxDataContext.OutboxMessages
            .Where(x => x.ProcessedOn == null).ToListAsync(cancellationToken: cancellationToken);

        if (!unsentMessages.Any())
        {
            _logger.LogTrace("No unsent messages found in outbox");
            return;
        }

        _logger.LogInformation(
            "Found {Count} unsent messages in outbox, sending...",
            unsentMessages.Count);

        foreach (var outboxMessage in unsentMessages)
        {
            var type = Type.GetType(outboxMessage.Type);

            Guard.Against.Null(type, nameof(type));

            dynamic? data = _messageSerializer.Deserialize(outboxMessage.Data, type);
            if (data is null)
            {
                _logger.LogError("Invalid message type: {Name}", type?.Name);
                continue;
            }

            if (outboxMessage.EventType == EventType.DomainNotificationEvent &&
                data is IDomainNotificationEvent domainNotificationEvent)
            {
                // domain event notification
                await _mediator.Publish(domainNotificationEvent, cancellationToken);

                _logger.LogInformation(
                    "Dispatched a notification: '{Name}' with ID: '{Id} (outbox)'",
                    outboxMessage.Name,
                    domainNotificationEvent.EventId);
            }

            if (outboxMessage.EventType == EventType.IntegrationEvent && data is IIntegrationEvent integrationEvent)
            {
                // integration event
                await _eventBusPublisher.PublishAsync(integrationEvent, cancellationToken);

                _logger.LogInformation(
                    "Publish a message: '{Name}' with ID: '{Id} (outbox)'",
                    outboxMessage.Name,
                    integrationEvent?.EventId);
            }

            outboxMessage.MarkAsProcessed();
        }

        await _outboxDataContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
