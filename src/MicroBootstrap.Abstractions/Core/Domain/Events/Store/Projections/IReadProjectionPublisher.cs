using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;

namespace MicroBootstrap.Abstractions.Core.Domain.Events.Store.Projections;

public interface IReadProjectionPublisher
{
    Task PublishAsync(IStreamEvent streamEvent, CancellationToken cancellationToken = default);

    Task PublishAsync<T>(IStreamEvent<T> streamEvent, CancellationToken cancellationToken = default)
        where T : IDomainEvent;
}