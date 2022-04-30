using System.Diagnostics.Eventing.Reader;
using Marten;
using Marten.Events;
using MicroBootstrap.Abstractions.Core.Domain.Events;

namespace MicroBootstrap.Persistence.Marten.Subscriptions;

public class MartenEventPublisher : IMartenEventsConsumer
{
    private readonly IServiceProvider _serviceProvider;

    public MartenEventPublisher(
        IServiceProvider serviceProvider
    )
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ConsumeAsync(
        IDocumentOperations documentOperations,
        IReadOnlyList<StreamAction> streamActions,
        CancellationToken cancellationToken = default)
    {
        foreach (var @event in streamActions.SelectMany(streamAction => streamAction.Events))
        {
            using var scope = _serviceProvider.CreateScope();
            var eventBus = scope.ServiceProvider.GetRequiredService<IEventProcessor>();

            // var streamEvent = @event.ToStreamEvent();
            // await eventBus.PublishAsync(streamEvent, cancellationToken);
        }
    }
}