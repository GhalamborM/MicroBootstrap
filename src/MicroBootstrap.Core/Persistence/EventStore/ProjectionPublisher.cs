using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Persistence.EventStore;
using MicroBootstrap.Abstractions.Persistence.EventStore.Projections;

namespace MicroBootstrap.Core.Persistence.EventStore;

public class ReadProjectionPublisher : IReadProjectionPublisher
{
    private readonly IServiceProvider _serviceProvider;

    public ReadProjectionPublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync<T>(IStreamEvent<T> streamEvent, CancellationToken cancellationToken = default)
        where T : IDomainEvent
    {
        using var scope = _serviceProvider.CreateScope();
        var projections = scope.ServiceProvider.GetRequiredService<IEnumerable<IHaveReadProjection>>();
        foreach (var projection in projections)
        {
            await projection.ProjectAsync(streamEvent, cancellationToken);
        }
    }

    public Task PublishAsync(IStreamEvent streamEvent, CancellationToken cancellationToken = default)
    {
        var streamData = streamEvent.Data.GetType();

        var method = typeof(IReadProjectionPublisher)
            .GetMethods()
            .Single(m => m.Name == nameof(PublishAsync) && m.GetGenericArguments().Any())
            .MakeGenericMethod(streamData);

        return (Task)method
            .Invoke(this, new object[] { streamEvent, cancellationToken })!;
    }
}