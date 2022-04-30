using System.Linq.Expressions;

namespace MicroBootstrap.Abstractions.Persistence.EfCore.Specification;

public interface IRootSpecification
{
}

// https://stackoverflow.com/questions/63082758/ef-core-specification-pattern-add-all-column-for-sorting-data-with-custom-specif
public interface ISpecification<T> : IRootSpecification
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    Expression<Func<T, object>> OrderBy { get; }
    Expression<Func<T, object>> OrderByDescending { get; }
    Expression<Func<T, object>> GroupBy { get; }

    int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; }

    bool IsSatisfiedBy(T obj);
}
