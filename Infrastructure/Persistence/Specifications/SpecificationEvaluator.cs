using Core.Application.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Specifications;

public static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(IQueryable<T> inputQuery, ISpecification<T> specification) where T : class
    {
        var query = inputQuery;
        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        IOrderedQueryable<T>? ordered = null;
        foreach (var order in specification.OrderByDescending)
        {
            ordered = ordered is null ? query.OrderByDescending(order) : ordered.ThenByDescending(order);
        }
        foreach (var order in specification.OrderBy)
        {
            ordered = ordered is null ? query.OrderBy(order) : ordered.ThenBy(order);
        }

        query = ordered ?? query;
        return specification.IsPagingEnabled ? query.Skip(specification.Skip).Take(specification.Take) : query;
    }
}
