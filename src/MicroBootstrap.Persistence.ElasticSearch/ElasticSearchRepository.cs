using System.Linq.Expressions;
using Ardalis.GuardClauses;
using MicroBootstrap.MicroBootstrap.Abstractions.Domain.Model;
using MicroBootstrap.MicroBootstrap.Abstractions.Persistence;
using Nest;

namespace MicroBootstrap.MicroBootstrap.Persistence.ElasticSearch;

public class ElasticSearchRepository<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : class, IHaveIdentity<TId>, new()
{
    private readonly IElasticClient _elasticClient;

    public ElasticSearchRepository(IElasticClient elasticClient)
    {
        _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
    }

    public async Task<TEntity?> FindByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(id, nameof(id));

        var result = id switch
        {
            string stringId => await _elasticClient.GetAsync<TEntity>(stringId, ct: cancellationToken),
            long longId => await _elasticClient.GetAsync<TEntity>(longId, ct: cancellationToken),
            int intId => await _elasticClient.GetAsync<TEntity>(intId, ct: cancellationToken),
            Guid guidId => await _elasticClient.GetAsync<TEntity>(guidId, ct: cancellationToken),
            _ => throw new ArgumentOutOfRangeException(
                nameof(id),
                $"{nameof(id)} has to be of type string, int, long or Guid")
        };

        return result?.Source;
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

    public Task<IReadOnlyList<TEntity>> RawQuery(
        string query,
        CancellationToken cancellationToken = default,
        params object[] queryParams)
    {
        throw new NotImplementedException();
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(entity, nameof(entity));

        _ = entity.Id switch
        {
            string stringId => await _elasticClient.IndexAsync(entity, i => i.Id(stringId), cancellationToken),
            long longId => await _elasticClient.IndexAsync(entity, i => i.Id(longId), cancellationToken),
            int intId => await _elasticClient.IndexAsync(entity, i => i.Id(intId), cancellationToken),
            Guid guidId => await _elasticClient.IndexAsync(entity, i => i.Id(guidId), cancellationToken),
            _ => throw
                new ArgumentOutOfRangeException(
                    nameof(entity),
                    $"{nameof(entity.Id)} has to be of type string, int, long or Guid")
        };

        return entity;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(entity, nameof(entity));

        _ = entity.Id switch
        {
            string stringId => await _elasticClient.UpdateAsync<TEntity>(
                stringId,
                i => i.Doc(entity),
                cancellationToken),
            long longId => await _elasticClient.UpdateAsync<TEntity>(longId, i => i.Doc(entity), cancellationToken),
            int intId => await _elasticClient.UpdateAsync<TEntity>(intId, i => i.Doc(entity), cancellationToken),
            Guid guidId => await _elasticClient.UpdateAsync<TEntity>(guidId, i => i.Doc(entity), cancellationToken),
            _ => throw new ArgumentOutOfRangeException(
                paramName: nameof(entity),
                message: $"{nameof(entity.Id)} has to be of type string, int, long or Guid")
        };
        return entity;
    }

    public Task DeleteRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(entity, nameof(entity));

        _ = entity.Id switch
        {
            string stringId => await _elasticClient.DeleteAsync<TEntity>(stringId, ct: cancellationToken),
            long longId => await _elasticClient.DeleteAsync<TEntity>(longId, ct: cancellationToken),
            int intId => await _elasticClient.DeleteAsync<TEntity>(intId, ct: cancellationToken),
            Guid guidId => await _elasticClient.DeleteAsync<TEntity>(guidId, ct: cancellationToken),
            _ => throw new ArgumentOutOfRangeException(
                nameof(entity),
                $"{nameof(entity.Id)} has to be of type string, int, long or Guid")
        };
    }

    public async Task DeleteByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(id, nameof(id));

        _ = id switch
        {
            string stringId => await _elasticClient.DeleteAsync<TEntity>(stringId, ct: cancellationToken),
            long longId => await _elasticClient.DeleteAsync<TEntity>(longId, ct: cancellationToken),
            int intId => await _elasticClient.DeleteAsync<TEntity>(intId, ct: cancellationToken),
            Guid guidId => await _elasticClient.DeleteAsync<TEntity>(guidId, ct: cancellationToken),
            _ => throw new ArgumentOutOfRangeException(
                nameof(id),
                $"{nameof(id)} has to be of type string, int, long or Guid")
        };
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
