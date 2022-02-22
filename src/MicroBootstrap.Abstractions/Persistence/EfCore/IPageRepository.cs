using MicroBootstrap.Abstractions.Domain.Model;
using MicroBootstrap.Abstractions.Persistence.EfCore.Specification;

namespace MicroBootstrap.Abstractions.Persistence.EfCore;

public interface IPageRepository<TEntity, TKey>
    where TEntity : IHaveIdentity<TKey>
{
    ValueTask<long> CountAsync(IPageSpecification<TEntity> spec, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> FindAsync(IPageSpecification<TEntity> spec, CancellationToken cancellationToken = default);
}

public interface IPageRepository<TEntity> : IPageRepository<TEntity, Guid>
    where TEntity : IHaveIdentity<Guid>
{
}
