using System.Data;
using Dapper;

namespace MicroBootstrap.Persistence.Dapper;

public class DapperRepository<TEntity> : IDapperRepository<TEntity>
    where TEntity : class

{
    private readonly IDbConnection _connection;
    public DapperRepository(IDbConnection connection)
    {
        _connection = connection;
    }


    public IEnumerable<TEntity> Query(string? query = null, object? param = null)
    {

        using (_connection)
        {
            _connection.Open();
            return _connection.Query<TEntity>(query, param);
        }
    }

    public async Task<IEnumerable<TEntity>> QueryAsync(string? query = null, object? param = null)
    {
        using (_connection)
        {
            _connection.Open();
            return await _connection.QueryAsync<TEntity>(query, param);
        }
    }

    public void Execute(string? query = null, object? param = null)
    {

        using (_connection)
        {
            _connection.Open();
            _connection.Execute(query, param);
        }
    }

    public async Task ExecuteAsync(string? query = null, object? param = null)
    {
        using (_connection)
        {
            _connection.Open();
            await _connection.ExecuteAsync(query, param);
        }
    }

    public void Dispose()
    {
        _connection.Close();
    }
}
