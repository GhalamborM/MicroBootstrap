using Marten;
using MicroBootstrap.Abstractions.Core.Domain.Events;
using MicroBootstrap.Abstractions.Core.Domain.Model;

namespace MicroBootstrap.Persistence.Marten.Extensions;

public static class AggregateExtensions
{
    public static async Task StoreAndPublishEvents(
        this IAggregate<Guid> aggregate,
        IDocumentSession session,
        IEventProcessor eventProcessor,
        CancellationToken cancellationToken = default
    )
    {
        var uncommittedEvents = aggregate.FlushUncommittedEvents().ToArray();
        session.Events.Append(aggregate.Id, uncommittedEvents);
        await session.SaveChangesAsync(cancellationToken);
        await eventProcessor.PublishAsync(uncommittedEvents, cancellationToken);
    }
}
