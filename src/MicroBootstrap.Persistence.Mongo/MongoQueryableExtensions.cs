using System.Linq.Expressions;
using FilterModel = MicroBootstrap.MicroBootstrap.Abstractions.CQRS.FilterModel;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace MicroBootstrap.Persistence.Mongo;

public static class MongoQueryableExtensions
{
    public static async Task<ListResultModel<T>> PaginateAsync<T>(
        this IMongoQueryable<T> collection,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (page <= 0) page = 1;

        if (pageSize <= 0) pageSize = 10;

        var isEmpty = await collection.AnyAsync(cancellationToken: cancellationToken) == false;
        if (isEmpty) return ListResultModel<T>.Empty;

        var totalItems = await collection.CountAsync(cancellationToken: cancellationToken);
        var totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
        var data = await collection.Limit(page, pageSize).ToListAsync(cancellationToken: cancellationToken);

        return ListResultModel<T>.Create(data, totalItems, page, pageSize);
    }

    public static async Task<ListResultModel<R>> PaginateAsync<T, R>(
        this IMongoQueryable<T> collection,
        IConfigurationProvider configuration,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (page <= 0) page = 1;

        if (pageSize <= 0) pageSize = 10;

        var isEmpty = await collection.AnyAsync(cancellationToken: cancellationToken) == false;
        if (isEmpty) return ListResultModel<R>.Empty;

        var totalItems = await collection.CountAsync(cancellationToken: cancellationToken);
        var totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
        var data = collection.Limit(page, pageSize).ProjectTo<R>(configuration).ToList();

        return ListResultModel<R>.Create(data, totalItems, page, pageSize);
    }

    public static IMongoQueryable<T> Limit<T>(
        this IMongoQueryable<T> collection,
        int page = 1,
        int resultsPerPage = 10)
    {
        if (page <= 0) page = 1;

        if (resultsPerPage <= 0) resultsPerPage = 10;

        var skip = (page - 1) * resultsPerPage;
        var data = collection.Skip(skip)
            .Take(resultsPerPage);

        return data;
    }

    public static IMongoQueryable<TEntity> ApplyFilterList<TEntity>(
        this IMongoQueryable<TEntity> source,
        IEnumerable<FilterModel>? filters)
        where TEntity : class
    {
        if (filters is null)
            return source;

        List<Expression<Func<TEntity, bool>>> filterExpressions = new List<Expression<Func<TEntity, bool>>>();

        foreach (var (fieldName, comparision, fieldValue) in filters)
        {
            Expression<Func<TEntity, bool>> expr = PredicateBuilder.Build<TEntity>(fieldName, comparision, fieldValue);
            filterExpressions.Add(expr);
        }

        return source.Where(filterExpressions.Aggregate((expr1, expr2) => expr1.And(expr2)));
    }

    public static IMongoQueryable<TEntity> ApplyPaging<TEntity>(
        this IMongoQueryable<TEntity> source,
        int page,
        int size)
        where TEntity : class
    {
        return source.Skip(page * size).Take(size);
    }
}
