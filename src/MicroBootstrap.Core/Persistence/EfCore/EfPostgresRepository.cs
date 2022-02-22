namespace MicroBootstrap.Core.Persistence.EfCore;

public class EfRepository<TDbContext, TEntity, TKey> : EfRepositoryBase<TDbContext, TEntity, TKey>
    where TEntity : class, IHaveIdentity<TKey>
    where TDbContext : DbContext
{
    public EfRepository(TDbContext dbContext, IAggregatesDomainEventsStore aggregatesDomainEventsStore)
        : base(dbContext, aggregatesDomainEventsStore)
    {
    }
}

public class EfRepository<TDbContext, TEntity> : EfRepository<TDbContext, TEntity, Guid>
    where TEntity : class, IHaveIdentity<Guid>
    where TDbContext : DbContext
{
    public EfRepository(TDbContext dbContext, [NotNull] IAggregatesDomainEventsStore aggregatesDomainEventsStore)
        : base(dbContext, aggregatesDomainEventsStore)
    {
    }
}
