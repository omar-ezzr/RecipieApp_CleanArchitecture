using System.Linq.Expressions;

namespace Core.Application.Specifications;

public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }
    IReadOnlyList<Expression<Func<T, object>>> OrderBy { get; }
    IReadOnlyList<Expression<Func<T, object>>> OrderByDescending { get; }
    int Skip { get; }
    int Take { get; }
    bool IsPagingEnabled { get; }
}
