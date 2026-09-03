using Core.Application.DTO;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Application.Specifications.Recipes;

public sealed class RecipeFilterSpecification : Specification<Recipie>
{
    public RecipeFilterSpecification(RecipeQueryParams parameters, bool applyPaging = true)
    {
        var search = parameters.Search?.Trim();
        var hasDifficulty = Enum.TryParse<DifficultyLevel>(parameters.Difficulty, true, out var difficulty)
            && Enum.IsDefined(difficulty);

        Criteria = recipe =>
            (string.IsNullOrEmpty(search) || recipe.Title.Contains(search)) &&
            (!parameters.CategoryId.HasValue || recipe.CategoryId == parameters.CategoryId.Value) &&
            (!parameters.UserId.HasValue || recipe.UserId == parameters.UserId.Value) &&
            (!parameters.CuisineId.HasValue || recipe.CuisineId == parameters.CuisineId.Value) &&
            (!parameters.RegionId.HasValue || recipe.RegionId == parameters.RegionId.Value) &&
            (!parameters.IsTraditional.HasValue || recipe.IsTraditional == parameters.IsTraditional.Value) &&
            (!hasDifficulty || recipe.Difficulty == difficulty);

        switch (parameters.SortBy?.Trim().ToLowerInvariant())
        {
            case "title":
                AddOrderBy(recipe => recipe.Title);
                AddOrderBy(recipe => recipe.Id);
                break;
            case "time":
                AddOrderBy(recipe => recipe.PreparationTimeMinutes);
                AddOrderBy(recipe => recipe.Id);
                break;
            case "difficulty":
                AddOrderBy(recipe => recipe.Difficulty);
                AddOrderBy(recipe => recipe.Id);
                break;
            default:
                AddOrderByDescending(recipe => recipe.CreatedAt);
                AddOrderByDescending(recipe => recipe.Id);
                break;
        }

        if (applyPaging)
        {
            var page = Math.Max(1, parameters.Page);
            var pageSize = Math.Min(100, parameters.PageSize < 1 ? 10 : parameters.PageSize);
            ApplyPaging((page - 1) * pageSize, pageSize);
        }
    }
}
