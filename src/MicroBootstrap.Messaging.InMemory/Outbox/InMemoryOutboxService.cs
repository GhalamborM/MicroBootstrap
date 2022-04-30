using Ardalis.GuardClauses;
using Humanizer;
using MediatR;
using MicroBootstrap.Abstractions.Core.Domain.Events.External;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Messaging.Outbox;
using MicroBootstrap.Abstractions.Messaging.Serialization;
using MicroBootstrap.Abstractions.Messaging.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MicroBootstrap.Messaging.InMemory.Outbox;

public class InMemoryOutboxService : IOutboxService
{
    private readonly OutboxOptions _options;
    private readonly ILogger<InMemoryOutboxService> _logger;
    private readonly IMessageSerializer _messageSerializer;
    private readonly IMediator _mediator;
    private readonly IEventBusPublisher _eventBusPublisher;
    private readonly IInMemoryOutboxStore _inMemoryOutboxStore;

    public InMemoryOutboxService(
        IOptions<OutboxOptions> options,
        ILogger<InMemoryOutboxService> logger,
        IMessageSerializer messageSerializer,
        IMediator mediator,
        IEventBusPublisher eventBusPublisher,
        IInMemoryOutboxStore inMemoryOutboxStore)
    {
        _options = options.Value;
        _logger = logger;
        _messageSerializer = messageSerializer;
        _mediator = mediator;
        _eventBusPublisher = eventBusPublisher;
        _inMemoryOutboxStore = inMemoryOutboxStore;
    }

    public Task<IEnumerable<OutboxMessage>> GetAllUnsentOutboxMessagesAsync(
        EventType eventType = EventType.IntegrationEvent | EventType.DomainNotificationEvent,
        CancellationToken cancellationToken = default)
    {
        var messages = _inMemoryOutboxStore.Events
            .Where(x => x.EventType == eventType && x.ProcessedOn == null);

        return Task.FromResult(messages);
    }

    public Task<IEnumerable<OutboxMessage>> GetAllOutboxMessagesAsync(
        EventType eventType = EventType.IntegrationEvent | EventType.DomainNotificationEvent,
        CancellationToken cancellationToken = default)
    {
        var messages = _inMemoryOutboxStore.Events.Where(x => x.EventType == eventType);

        return Task.FromResult(messages);
    }

    public Task CleanProcessedAsync(CancellationToken cancellationToken = default)
    {
        _inMemoryOutboxStore.Events.ToList().RemoveAll(x => x.ProcessedOn != null);

        return Task.CompletedTask;
    }

    public async Task SaveAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(integrationEvent, nameof(integrationEvent));
        await SaveAsync(new[] { integrationEvent }, cancellationToken);
    }

    public Task SaveAsync(IIntegrationEvent[] integrationEvents, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(integrationEvents, nameof(integrationEvents));

        if (integrationEvents.Any() == false)
            return Task.CompletedTask;

        if (!_options.Enabled)
        {
            _logger.LogWarning("Outbox is disabled, outgoing messages won't be saved");
            return Task.CompletedTask;
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

            _inMemoryOutboxStore.Events.Add(outboxMessages);
        }

        _logger.LogInformation("Saved message to the outbox");

        return Task.CompletedTask;
    }

    public async Task SaveAsync(
        IDomainNotificationEvent domainNotificationEvent,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(domainNotificationEvent, nameof(domainNotificationEvent));

        await SaveAsync(new[] { domainNotificationEvent }, cancellationToken);
    }

    public Task SaveAsync(
        IDomainNotificationEvent[] domainNotificationEvents,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(domainNotificationEvents, nameof(domainNotificationEvents));

        if (domainNotificationEvents.Any() == false)
        {
            return Task.CompletedTask;
        }

        if (!_options.Enabled)
        {
            _logger.LogWarning("Outbox is disabled, outgoing messages won't be saved");
            return Task.CompletedTask;
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

            _inMemoryOutboxStore.Events.Add(outboxMessages);
        }

        _logger.LogInformation("Saved message to the outbox");

        return Task.CompletedTask;
    }

    public async Task PublishUnsentOutboxMessagesAsync(CancellationToken cancellationToken = default)
    {
        var unsentMessages = _inMemoryOutboxStore.Events.Where(x => x.ProcessedOn == null).ToList();

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

            var data = _messageSerializer.Deserialize(outboxMessage.Data, type);

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
                    "Published a notification: '{Name}' with ID: '{Id} (outbox)'",
                    outboxMessage.Name,
                    domainNotificationEvent?.EventId);
            }

            if (outboxMessage.EventType == EventType.IntegrationEvent && data is IIntegrationEvent integrationEvent)
            {
                // integration event
                await _eventBusPublisher.PublishAsync(integrationEvent, cancellationToken);

                _logger.LogInformation(
                    "Published a message: '{Name}' with ID: '{Id} (outbox)'",
                    outboxMessage.Name,
                    integrationEvent?.EventId);
            }

            outboxMessage.MarkAsProcessed();
        }
    }
}
