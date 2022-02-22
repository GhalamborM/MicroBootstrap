namespace MicroBootstrap.Abstractions.Persistence;

/// <summary>
/// The unit of work pattern.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}

public interface IUnitOfWork<TContext> : IDisposable
    where TContext : class
{
    TContext Context { get; }
    Task CommitAsync(CancellationToken cancellationToken = default);
}
