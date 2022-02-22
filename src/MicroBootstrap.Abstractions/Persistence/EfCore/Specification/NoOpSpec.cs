using System.Linq.Expressions;

namespace MicroBootstrap.Abstractions.Persistence.EfCore.Specification;

public class NoOpSpec<TEntity> : SpecificationBase<TEntity>
{
    public override Expression<Func<TEntity, bool>> Criteria => p => true;
}
