using System.Collections.Immutable;
using Ardalis.GuardClauses;
using MediatR;
using MicroBootstrap.Abstractions.Core.Domain.Events;
using MicroBootstrap.Abstractions.Core.Domain.Events.External;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Core.Extensions.Events;
using MicroBootstrap.Core.Extensions.Mediator;

namespace MicroBootstrap.Core.Domain.Events;

public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly IDomainNotificationEventPublisher _domainNotificationEventPublisher;
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;

    public DomainEventPublisher(
        IMediator mediator,
        IIntegrationEventPublisher integrationEventPublisher,
        IDomainNotificationEventPublisher domainNotificationEventPublisher,
        IServiceProvider serviceProvider)
    {
        _domainNotificationEventPublisher =
            Guard.Against.Null(domainNotificationEventPublisher, nameof(domainNotificationEventPublisher));
        _integrationEventPublisher = Guard.Against.Null(integrationEventPublisher, nameof(integrationEventPublisher));
        _mediator = Guard.Against.Null(mediator, nameof(mediator));
        _serviceProvider = Guard.Against.Null(serviceProvider, nameof(serviceProvider));
    }

    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return PublishAsync(new[] { domainEvent }, cancellationToken);
    }

    public async Task PublishAsync(IDomainEvent[] domainEvents, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(domainEvents, nameof(domainEvents));

        if (domainEvents.Any() == false)
            return;

        var domainEventContext = _serviceProvider.GetRequiredService<IDomainEventContext>();

        // https://github.com/dotnet-architecture/eShopOnContainers/issues/700#issuecomment-461807560
        // https://github.com/dotnet-architecture/eShopOnContainers/blob/e05a87658128106fef4e628ccb830bc89325d9da/src/Services/Ordering/Ordering.Infrastructure/OrderingContext.cs#L65
        // http://www.kamilgrzybek.com/design/how-to-publish-and-handle-domain-events/
        // http://www.kamilgrzybek.com/design/handling-domain-events-missing-part/
        // https://www.ledjonbehluli.com/posts/domain_to_integration_event/

        // Dispatch our domain events before commit
        IReadOnlyList<IDomainEvent> eventsToDispatch = domainEvents.ToList();

        if (eventsToDispatch.Any() == false)
        {
            eventsToDispatch = domainEventContext.GetAllUncommittedEvents().ToImmutableList();
        }

        await _mediator.DispatchDomainEventAsync(eventsToDispatch, cancellationToken: cancellationToken);

        // Save wrapped integration and notification events to outbox for further processing after commit
        var wrappedNotificationEvents = eventsToDispatch.GetWrappedDomainNotificationEvents().ToArray();
        await _domainNotificationEventPublisher.PublishAsync(wrappedNotificationEvents.ToArray(), cancellationToken);

        var wrappedIntegrationEvents = eventsToDispatch.GetWrappedIntegrationEvents().ToArray();
        await _integrationEventPublisher.PublishAsync(wrappedIntegrationEvents.ToArray(), cancellationToken);

        IReadOnlyList<IEventMapper> eventMappers = _serviceProvider.GetServices<IEventMapper>().ToImmutableList();

        // Save event mapper events into outbox for further processing after commit
        var integrationEvents =
            GetIntegrationEvents(_serviceProvider, eventMappers, eventsToDispatch);
        if (integrationEvents.Any())
        {
            await _integrationEventPublisher.PublishAsync(integrationEvents.ToArray(), cancellationToken);
        }

        var notificationEvents =
            GetNotificationEvents(_serviceProvider, eventMappers, eventsToDispatch);

        if (notificationEvents.Any())
        {
            await _domainNotificationEventPublisher.PublishAsync(notificationEvents.ToArray()!, cancellationToken);
        }
    }


    private IReadOnlyList<IDomainNotificationEvent> GetNotificationEvents(
        IServiceProvider serviceProvider,
        IReadOnlyList<IEventMapper> eventMappers,
        IReadOnlyList<IDomainEvent> eventsToDispatch)
    {
        IReadOnlyList<IIDomainNotificationEventMapper> notificationEventMappers =
            serviceProvider.GetServices<IIDomainNotificationEventMapper>().ToImmutableList();

        List<IDomainNotificationEvent> notificationEvents = new List<IDomainNotificationEvent>();

        if (eventMappers.Any())
        {
            foreach (var eventMapper in eventMappers)
            {
                var items = eventMapper.MapToDomainNotificationEvents(eventsToDispatch)?.ToList();
                if (items is not null && items.Any())
                {
                    notificationEvents.AddRange(items.Where(x => x is not null)!);
                }
            }
        }
        else if (notificationEventMappers.Any())
        {
            foreach (var notificationEventMapper in notificationEventMappers)
            {
                var items = notificationEventMapper.MapToDomainNotificationEvents(eventsToDispatch)?.ToList();
                if (items is not null && items.Any())
                {
                    notificationEvents.AddRange(items.Where(x => x is not null)!);
                }
            }
        }

        return notificationEvents.ToImmutableList();
    }

    private static IReadOnlyList<IIntegrationEvent> GetIntegrationEvents(
        IServiceProvider serviceProvider,
        IReadOnlyList<IEventMapper> eventMappers,
        IReadOnlyList<IDomainEvent> eventsToDispatch)
    {
        IReadOnlyList<IIntegrationEventMapper> integrationEventMappers =
            serviceProvider.GetServices<IIntegrationEventMapper>().ToImmutableList();

        List<IIntegrationEvent> integrationEvents = new List<IIntegrationEvent>();

        if (eventMappers.Any())
        {
            foreach (var eventMapper in eventMappers)
            {
                var items = eventMapper.MapToIntegrationEvents(eventsToDispatch)?.ToList();
                if (items is not null && items.Any())
                {
                    integrationEvents.AddRange(items.Where(x => x is not null)!);
                }
            }
        }
        else if (integrationEventMappers.Any())
        {
            foreach (var integrationEventMapper in integrationEventMappers)
            {
                var items = integrationEventMapper.MapToIntegrationEvents(eventsToDispatch)?.ToList();
                if (items is not null && items.Any())
                {
                    integrationEvents.AddRange(items.Where(x => x is not null)!);
                }
            }
        }

        return integrationEvents.ToImmutableList();
    }
}
