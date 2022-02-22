namespace MicroBootstrap.Persistence.Dapper
{
    public interface IDapperRepository<TEntity> : IDisposable
        where TEntity : class
    {
        IEnumerable<TEntity> Query(string? query = null, object? param = null);
        Task<IEnumerable<TEntity>> QueryAsync(string? query = null, object? param = null);
        void Execute(string? query = null, object? param = null);
        Task ExecuteAsync(string? query = null, object? param = null);
    }
}
