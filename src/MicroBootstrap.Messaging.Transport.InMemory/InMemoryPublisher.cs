using MicroBootstrap.Abstractions.Core.Domain.Events.External;
using MicroBootstrap.Abstractions.Messaging.Transport;
using MicroBootstrap.Messaging.Transport.InMemory.Channels;
using MicroBootstrap.Messaging.Transport.InMemory.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MicroBootstrap.Messaging.Transport.InMemory;

public class InMemoryPublisher : IEventBusPublisher
{
    private readonly ILogger<InMemoryPublisher> _logger;
    private readonly IMessageChannel _channel;
    private readonly InMemoryProducerDiagnostics _producerDiagnostics;

    public InMemoryPublisher(
        ILogger<InMemoryPublisher> logger,
        IMessageChannel channel,
        InMemoryProducerDiagnostics producerDiagnostics)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _channel = channel;
        _producerDiagnostics = producerDiagnostics;
    }

    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        if (integrationEvent == null)
            throw new ArgumentNullException(nameof(integrationEvent));

        // if (message.CorrelationId == Guid.Empty)
        // {
        //     var context = _serviceProvider.GetRequiredService<ICorrelationContextAccessor>();
        //     message.CorrelationId = Guid.Parse(context.CorrelationId);
        // }
        if (_channel.Writer is not null)
        {
            _logger.LogInformation("publishing message '{message.Id}'...", integrationEvent.EventId);

            // ProducerDiagnostics
            _producerDiagnostics.StartActivity(integrationEvent);
            await _channel.Writer.WriteAsync(integrationEvent, cancellationToken);
            _producerDiagnostics.StopActivity(integrationEvent);
        }
        else
        {
            _logger.LogWarning(
                "no suitable publisher found for message '{message.Id}' with type '{typeof(T).FullName}' !",
                integrationEvent.EventId,
                typeof(TEvent).FullName);
            _producerDiagnostics.NoSubscriberToPublish(integrationEvent);
        }
    }
}
