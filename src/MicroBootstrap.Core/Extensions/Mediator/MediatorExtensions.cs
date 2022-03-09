using Ardalis.GuardClauses;
using MediatR;
using MicroBootstrap.Abstractions.Core.Domain.Events.External;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Messaging.Serialization;
using MicroBootstrap.Abstractions.Scheduler;
using MicroBootstrap.Core.Dependency;
using MicroBootstrap.Core.Extensions.Utils.Reflections;
using Newtonsoft.Json;
using Serilog;

namespace MicroBootstrap.Core.Extensions.Mediator;

public static class MediatorExtensions
{
    public static Task DispatchDomainEventAsync(
        this IMediator mediator,
        IReadOnlyList<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(domainEvents, nameof(domainEvents));

        var tasks = domainEvents
            .Select(async domainEvent =>
            {
                await DispatchDomainEventAsync(mediator, domainEvent, cancellationToken);
            });

        return Task.WhenAll(tasks);
    }

    public static async Task DispatchDomainEventAsync(
        this IMediator mediator,
        IDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(domainEvent, nameof(domainEvent));
        var serializer = ServiceActivator.GetRequiredService<IMessageSerializer>();

        await mediator.Publish(domainEvent, cancellationToken);
        Log.Logger.Debug(
            "Published domain event {DomainEventName} with payload {DomainEventContent}",
            domainEvent.GetType().FullName,
            serializer.Serialize(domainEvent));
    }

    public static Task DispatchDomainNotificationEventAsync(
        this IMediator mediator,
        IList<IDomainNotificationEvent> domainNotificationEvents,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(domainNotificationEvents, nameof(domainNotificationEvents));

        var tasks = domainNotificationEvents
            .Select(async domainNotificationEvent =>
            {
                await DispatchDomainNotificationEventAsync(mediator, domainNotificationEvent, cancellationToken);
            });
        return Task.WhenAll(tasks);
    }

    public static async Task DispatchDomainNotificationEventAsync(
        this IMediator mediator,
        IDomainNotificationEvent domainNotificationEvent,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(domainNotificationEvent, nameof(domainNotificationEvent));
        var serializer = ServiceActivator.GetRequiredService<IMessageSerializer>();

        await mediator.Publish(domainNotificationEvent, cancellationToken);
        Log.Logger.Debug(
            "Published domain notification event {DomainNotificationEventName} with payload {DomainNotificationEventContent}",
            domainNotificationEvent.GetType().FullName,
            serializer.Serialize(domainNotificationEvent));
    }

    public static Task DispatchIntegrationEventAsync(
        this IMediator mediator,
        IReadOnlyList<IIntegrationEvent> integrationEvents,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(integrationEvents, nameof(integrationEvents));

        var tasks = integrationEvents
            .Select(async integrationEvent =>
            {
                await DispatchIntegrationEventAsync(mediator, integrationEvent, cancellationToken);
            });

        return Task.WhenAll(tasks);
    }

    public static async Task DispatchIntegrationEventAsync(
        this IMediator mediator,
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(integrationEvent, nameof(integrationEvent));

        var serializer = ServiceActivator.GetRequiredService<IMessageSerializer>();

        await mediator.Publish(integrationEvent, cancellationToken);
        Log.Logger.Debug(
            "Published integration notification event {IntegrationEventName} with payload {IntegrationEventContent}",
            integrationEvent.GetType().FullName,
            serializer.Serialize(integrationEvent));
    }

    public static async Task SendScheduleObject(
        this IMediator mediator,
        ScheduleSerializedObject scheduleSerializedObject)
    {
        var type = scheduleSerializedObject.GetPayloadType();

        dynamic? req = JsonConvert.DeserializeObject(scheduleSerializedObject.Data, type);

        if (req is null)
        {
            return;
        }

        await mediator.Send(req);
    }
}
