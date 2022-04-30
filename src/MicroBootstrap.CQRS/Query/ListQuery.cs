using MicroBootstrap.Abstractions.CQRS;
using MicroBootstrap.Abstractions.CQRS.Query;

namespace MicroBootstrap.CQRS.Query;

public record ListQuery<TResponse> : IListQuery<TResponse>
    where TResponse : notnull
{
    public IList<string>? Includes { get; init; }
    public IList<FilterModel>? Filters { get; init; }
    public IList<string>? Sorts { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}
