using System.Linq.Expressions;

namespace MicroBootstrap.Core.Persistence.EfCore;

public abstract class EfRepositoryBase<TDbContext, TEntity, TKey> :
    IEfRepository<TEntity, TKey>,
    IPageRepository<TEntity, TKey>
    where TEntity : class, IHaveIdentity<TKey>
    where TDbContext : DbContext
{
    protected readonly TDbContext DbContext;
    private readonly IAggregatesDomainEventsStore _aggregatesDomainEventsStore;
    protected readonly DbSet<TEntity> DbSet;

    protected EfRepositoryBase(TDbContext dbContext, IAggregatesDomainEventsStore aggregatesDomainEventsStore)
    {
        DbContext = dbContext;
        _aggregatesDomainEventsStore = aggregatesDomainEventsStore;
        DbSet = dbContext.Set<TEntity>();
    }

    public async ValueTask<long> CountAsync(
        IPageSpecification<TEntity> spec,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(spec, nameof(spec));

        spec.IsPagingEnabled = false;
        var specificationResult = GetQuery(DbSet, spec);
        return await ValueTask.FromResult(
            await specificationResult.LongCountAsync(cancellationToken));
    }

    public async Task<IReadOnlyList<TEntity>> FindAsync(
        IPageSpecification<TEntity> spec,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(spec, nameof(spec));

        var specificationResult = GetQuery(DbSet, spec);

        return await specificationResult.ToListAsync(cancellationToken);
    }

    public Task<TEntity?> FindByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return DbSet.SingleOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
    }


    public Task<TEntity?> FindOneAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(predicate, nameof(predicate));

        return DbSet.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public Task<TEntity?> FindOneAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(spec, nameof(spec));

        var specificationResult = GetQuery(DbSet, spec);
        return specificationResult.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> FindAsync(
        ISpecification<TEntity> spec,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(spec, nameof(spec));

        var specificationResult = GetQuery(DbSet, spec);
        return await specificationResult.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> RawQuery(
        string query,
        CancellationToken cancellationToken = default,
        params object[] queryParams)
    {
        Guard.Against.Null(query, nameof(query));
        Guard.Against.NullOrEmpty(queryParams, nameof(queryParams));

        return await DbSet.FromSqlRaw(query, queryParams).ToListAsync(cancellationToken);
    }

    public virtual IEnumerable<TEntity> GetInclude(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? includes = null, bool withTracking = true)
    {
        IQueryable<TEntity> query = DbSet;

        if (includes != null)
        {
            query = includes(query);
        }

        query = query.Where(predicate);

        if (withTracking == false)
        {
            query = query.Where(predicate).AsNoTracking();
        }

        return query.AsEnumerable();
    }

    public virtual IEnumerable<TEntity> GetInclude(
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? includes = null)
    {
        IQueryable<TEntity> query = DbSet;

        if (includes != null)
        {
            query = includes(query);
        }

        return query.AsEnumerable();
    }


    public virtual async Task<IEnumerable<TEntity>> GetIncludeAsync(
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? includes = null)
    {
        IQueryable<TEntity> query = DbSet;

        if (includes != null)
        {
            query = includes(query);
        }

        return await query.ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> GetIncludeAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? includes = null,
        bool withTracking = true)
    {
        IQueryable<TEntity> query = DbSet;

        if (includes != null)
        {
            query = includes(query);
        }

        query = query.Where(predicate);

        if (withTracking == false)
        {
            query = query.Where(predicate).AsNoTracking();
        }

        return await query.ToListAsync();
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(entity, nameof(entity));

        await DbSet.AddAsync(entity, cancellationToken);

        _aggregatesDomainEventsStore.AddEventsFrom(entity);

        return entity;
    }

    public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(entity, nameof(entity));

        var entry = DbContext.Entry(entity);
        entry.State = EntityState.Modified;

        _aggregatesDomainEventsStore.AddEventsFrom(entity);

        return Task.FromResult(entry.Entity);
    }

    public async Task DeleteRangeAsync(IReadOnlyList<TEntity> entities, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(entities, nameof(entities));

        foreach (var entity in entities)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }

    public Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var items = DbSet.Where(predicate).ToList();

        return DeleteRangeAsync(items, cancellationToken);
    }

    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(entity, nameof(entity));

        DbSet.Remove(entity);

        _aggregatesDomainEventsStore.AddEventsFrom(entity);

        return Task.CompletedTask;
    }

    public async Task DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var item = await DbSet.SingleOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
        Guard.Against.NotFound(id.ToString(), id.ToString(), nameof(id));

        DbSet.Remove(item);
    }

    private static IQueryable<TEntity> GetQuery(
        IQueryable<TEntity> inputQuery,
        ISpecification<TEntity> specification)
    {
        var query = inputQuery;

        if (specification.Criteria is not null) query = query.Where(specification.Criteria);

        query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

        query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        if (specification.OrderBy is not null)
            query = query.OrderBy(specification.OrderBy);
        else if (specification.OrderByDescending is not null)
            query = query.OrderByDescending(specification.OrderByDescending);

        if (specification.GroupBy is not null)
        {
            query = query
                .GroupBy(specification.GroupBy)
                .SelectMany(x => x);
        }

        if (specification.IsPagingEnabled)
        {
            query = query
                .Skip(specification.Skip - 1)
                .Take(specification.Take);
        }

        query = query.AsSplitQuery();

        return query;
    }

    private static IQueryable<TEntity> GetQuery(
        IQueryable<TEntity> inputQuery,
        IPageSpecification<TEntity> specification)
    {
        var query = inputQuery;

        if (specification.Criterias is not null && specification.Criterias.Count > 0)
        {
            var expr = specification.Criterias.First();
            for (var i = 1; i < specification.Criterias.Count; i++) expr = expr.And(specification.Criterias[i]);

            query = query.Where(expr);
        }

        query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

        query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        if (specification.OrderBy is not null)
            query = query.OrderBy(specification.OrderBy);
        else if (specification.OrderByDescending is not null)
            query = query.OrderByDescending(specification.OrderByDescending);

        if (specification.GroupBy is not null)
        {
            query = query
                .GroupBy(specification.GroupBy)
                .SelectMany(x => x);
        }

        if (specification.IsPagingEnabled)
        {
            query = query
                .Skip(specification.Skip - 1)
                .Take(specification.Take);
        }

        query = query.AsSplitQuery();

        return query;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
