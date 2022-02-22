using System.Linq.Expressions;
using MicroBootstrap.Abstractions.Domain.Model;
using MicroBootstrap.Abstractions.Persistence.EfCore.Specification;

namespace MicroBootstrap.Abstractions.Persistence.EfCore;

public interface IEfRepository<TEntity, in TId> : IRepository<TEntity, TId>, IDisposable
    where TEntity : class, IHaveIdentity<TId>
{
    Task<TEntity?> FindOneAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> FindAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default);

    IEnumerable<TEntity> GetInclude(Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? includes = null);

    IEnumerable<TEntity> GetInclude(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? includes = null,
        bool withTracking = true);

    Task<IEnumerable<TEntity>> GetIncludeAsync(
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? includes = null);

    Task<IEnumerable<TEntity>> GetIncludeAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? includes = null,
        bool withTracking = true);
}
