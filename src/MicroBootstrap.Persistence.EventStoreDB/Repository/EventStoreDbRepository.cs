using System.Linq.Expressions;

namespace MicroBootstrap.Persistence.EventStoreDB.Repository;

public class EventStoreDbRepository<T, TId> : IRepository<T, TId>
    where T : class, IHaveIdentity<TId>
{
    private readonly EventStoreClient _eventStoreDbClient;
    private readonly IEventProcessor _eventProcessor;

    public EventStoreDbRepository(
        EventStoreClient eventStoreDbClient,
        IEventProcessor eventProcessor
    )
    {
        _eventStoreDbClient = eventStoreDbClient;
        _eventProcessor = eventProcessor;
    }

    public Task<T?> FindByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<T>> RawQuery(string query, CancellationToken cancellationToken = default, params object[] queryParams)
    {
        throw new NotImplementedException();
    }

    public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteRangeAsync(IReadOnlyList<T> entities, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
