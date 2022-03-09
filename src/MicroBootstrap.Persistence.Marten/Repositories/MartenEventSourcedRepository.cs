using System.Linq.Expressions;
using Ardalis.GuardClauses;
using Marten;
using MicroBootstrap.Abstractions.Core.Domain.Events;
using MicroBootstrap.Abstractions.Core.Domain.Events.Store;
using MicroBootstrap.Abstractions.Core.Domain.Model;
using MicroBootstrap.Abstractions.Persistence;

namespace MicroBootstrap.Persistence.Marten.Repositories;

public class MartenEventSourcedRepository<TEntity> :
    IRepository<TEntity, Guid>
    where TEntity : class, IAggregate<Guid>, new()
{
    private readonly IAggregatesDomainEventsStore _aggregatesDomainEventsStore;
    private readonly IDocumentSession _documentSession;
    private readonly IEventStore _eventStore;

    public MartenEventSourcedRepository(
        IDocumentSession documentSession,
        MartenEventStore eventStore,
        IAggregatesDomainEventsStore aggregatesDomainEventsStore)
    {
        _aggregatesDomainEventsStore = aggregatesDomainEventsStore;
        _documentSession = Guard.Against.Null(documentSession, nameof(documentSession));
        _eventStore = Guard.Against.Null(eventStore, nameof(eventStore));
    }

    public async Task<TEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var streamState = await _documentSession.Events.FetchStreamStateAsync(id, cancellationToken);

        return streamState != null
            ? await _eventStore.Aggregate<TEntity>(id, cancellationToken)
            : null;
    }

    public Task<TEntity?> FindOneAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TEntity>> RawQuery(string query, CancellationToken cancellationToken = default,
        params object[] queryParams)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return StoreAsync(entity, null, cancellationToken);
    }

    public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return StoreAsync(entity, null, cancellationToken);
    }

    public Task DeleteRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return StoreAsync(entity, null, cancellationToken);
    }

    public Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private async Task<TEntity> StoreAsync(
        TEntity entity,
        int? expectedVersion,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(entity, nameof(entity));

        var entityEvents = _aggregatesDomainEventsStore.AddEventsFrom(entity);

        //await _eventStore.Append(entity.Id, expectedVersion, cancellationToken, entityEvents);

        return entity;
    }

    public void Dispose()
    {
    }
}
