using System.Collections.Immutable;
using System.Linq.Expressions;
using Ardalis.GuardClauses;
using Marten;
using MicroBootstrap.Abstractions.Core.Domain.Events;
using MicroBootstrap.Abstractions.Core.Domain.Model;
using MicroBootstrap.Abstractions.Persistence;

namespace MicroBootstrap.Persistence.Marten.Repositories;

public class MartenDocumentRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class, IHaveIdentity<TKey>
{
    private readonly IDocumentSession _documentSession;
    private readonly IAggregatesDomainEventsStore _aggregatesDomainEventsStore;

    public MartenDocumentRepository(
        IDocumentSession documentSession,
        IAggregatesDomainEventsStore aggregatesDomainEventsStore)
    {
        _documentSession = documentSession;
        _aggregatesDomainEventsStore = aggregatesDomainEventsStore;
    }


    public Task<TEntity?> FindByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(id, nameof(id));

        return id switch
        {
            Guid guid => _documentSession.LoadAsync<TEntity>(guid, cancellationToken),
            long l => _documentSession.LoadAsync<TEntity>(l, cancellationToken),
            int i => _documentSession.LoadAsync<TEntity>(i, cancellationToken),
            _ => _documentSession.LoadAsync<TEntity>(id.ToString()!, cancellationToken)
        };
    }

    public Task<TEntity?> FindOneAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(predicate, nameof(predicate));

        return _documentSession.Query<TEntity>().SingleOrDefaultAsync(predicate, token: cancellationToken);
    }

    public Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(predicate, nameof(predicate));

        return _documentSession.Query<TEntity>().Where(predicate).ToListAsync(cancellationToken);
    }

    public Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _documentSession.Query<TEntity>().ToListAsync(cancellationToken);
    }

    public Task<IReadOnlyList<TEntity>> RawQuery(
        string query,
        CancellationToken cancellationToken = default,
        params object[] queryParams)
    {
        Guard.Against.NullOrEmpty(query, nameof(query));
        Guard.Against.Null(queryParams, nameof(queryParams));

        return _documentSession.QueryAsync<TEntity>(query, cancellationToken, queryParams);
    }

    public Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(entity, nameof(entity));

        _documentSession.Insert(entity);

        _aggregatesDomainEventsStore.AddEventsFrom(entity);

        return Task.FromResult(entity);
    }

    public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(entity, nameof(entity));

        _documentSession.Update(entity);

        _aggregatesDomainEventsStore.AddEventsFrom(entity);

        return Task.FromResult(entity);
    }

    public async Task DeleteRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(entities, nameof(entities));
        foreach (var entity in entities)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }

    public async Task DeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(predicate, nameof(predicate));

        var entities = _documentSession.Query<TEntity>().Where(predicate);
        await DeleteRangeAsync(entities.ToImmutableList(), cancellationToken);
    }

    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(entity, nameof(entity));

        _documentSession.Delete(entity);

        _aggregatesDomainEventsStore.AddEventsFrom(entity);

        return Task.FromResult(entity);
    }

    public async Task DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(id, nameof(id));

        var entity = await FindByIdAsync(id, cancellationToken);
    }

    public void Dispose()
    {
        _documentSession.Dispose();
    }
}
