using System.Diagnostics.CodeAnalysis;
using MicroBootstrap.Abstractions.Core.Domain.Events;
using MicroBootstrap.Abstractions.Core.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace MicroBootstrap.Core.Persistence.EfCore;

public class EfRepository<TDbContext, TEntity, TKey> : EfRepositoryBase<TDbContext, TEntity, TKey>
    where TEntity : class, IHaveIdentity<TKey>
    where TDbContext : DbContext
{
    public EfRepository(TDbContext dbContext, IAggregatesDomainEventsRequestStore aggregatesDomainEventsStore)
        : base(dbContext, aggregatesDomainEventsStore)
    {
    }
}

public class EfRepository<TDbContext, TEntity> : EfRepository<TDbContext, TEntity, Guid>
    where TEntity : class, IHaveIdentity<Guid>
    where TDbContext : DbContext
{
    public EfRepository(TDbContext dbContext, [NotNull] IAggregatesDomainEventsRequestStore aggregatesDomainEventsStore)
        : base(dbContext, aggregatesDomainEventsStore)
    {
    }
}
