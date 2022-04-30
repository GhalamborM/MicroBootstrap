using System.Linq.Expressions;

namespace MicroBootstrap.Abstractions.Persistence.EfCore.Specification;

public interface IPageSpecification<T> : IRootSpecification
{
    List<Expression<Func<T, bool>>> Criterias { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    Expression<Func<T, object>> OrderBy { get; }
    Expression<Func<T, object>> OrderByDescending { get; }
    Expression<Func<T, object>> GroupBy { get; }

    int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; set; }
}
