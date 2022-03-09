using MediatR;
using MicroBootstrap.Abstractions.Core.Domain.Events;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.MicroBootstrap.Persistence.ElasticSearch.Indices;
using Nest;

namespace MicroBootstrap.MicroBootstrap.Persistence.ElasticSearch;

public class ElasticSearchProjection<TEvent, TView> : IEventHandler<TEvent>
    where TView : class, IHaveProjection
    where TEvent : IDomainEvent
{
    private readonly IElasticClient _elasticClient;
    private readonly Func<TEvent, string> _getId;

    public ElasticSearchProjection(
        IElasticClient elasticClient,
        Func<TEvent, string> getId
    )
    {
        _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        _getId = getId ?? throw new ArgumentNullException(nameof(getId));
    }

    public async Task Handle(TEvent @event, CancellationToken cancellationToken)
    {
        string id = _getId(@event);

        var entity = (await _elasticClient.GetAsync<TView>(id, ct: cancellationToken))?.Source
                     ?? (TView)Activator.CreateInstance(typeof(TView), true)!;

        entity.When(@event);

        var result = await _elasticClient.UpdateAsync<TView>(
            id,
            u => u.Doc(entity).Upsert(entity).Index(IndexNameMapper.ToIndexName<TView>()),
            cancellationToken
        );
    }
}

public static class ElasticSearchProjectionConfig
{
    public static IServiceCollection Project<TEvent, TView>(
        this IServiceCollection services,
        Func<TEvent, string> getId)
        where TView : class, IHaveProjection
        where TEvent : IDomainEvent
    {
        services.AddTransient<INotificationHandler<TEvent>>(sp =>
        {
            var session = sp.GetRequiredService<IElasticClient>();

            return new ElasticSearchProjection<TEvent, TView>(session, getId);
        });

        return services;
    }
}
