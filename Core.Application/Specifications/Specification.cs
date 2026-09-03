using System.Linq.Expressions;

namespace Core.Application.Specifications;

public abstract class Specification<T> : ISpecification<T>
{
    private readonly List<Expression<Func<T, object>>> _orderBy = [];
    private readonly List<Expression<Func<T, object>>> _orderByDescending = [];

    public Expression<Func<T, bool>>? Criteria { get; protected set; }
    public IReadOnlyList<Expression<Func<T, object>>> OrderBy => _orderBy;
    public IReadOnlyList<Expression<Func<T, object>>> OrderByDescending => _orderByDescending;
    public int Skip { get; private set; }
    public int Take { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    protected void AddOrderBy(Expression<Func<T, object>> expression) => _orderBy.Add(expression);
    protected void AddOrderByDescending(Expression<Func<T, object>> expression) => _orderByDescending.Add(expression);
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}
