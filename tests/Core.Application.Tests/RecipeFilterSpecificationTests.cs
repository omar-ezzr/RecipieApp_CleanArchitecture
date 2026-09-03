using Core.Application.DTO;
using Core.Application.Specifications.Recipes;
using Core.Domain.Enums;

namespace Core.Application.Tests;

public class RecipeFilterSpecificationTests
{
    [Fact]
    public void Uses_normalized_default_paging_and_deterministic_created_order()
    {
        var specification = new RecipeFilterSpecification(new RecipeQueryParams { Page = 0, PageSize = 0 });

        Assert.True(specification.IsPagingEnabled);
        Assert.Equal(0, specification.Skip);
        Assert.Equal(10, specification.Take);
        Assert.Equal(2, specification.OrderByDescending.Count);
    }

    [Theory]
    [InlineData("title")]
    [InlineData("time")]
    [InlineData("difficulty")]
    public void Uses_stable_secondary_ordering_for_explicit_sorts(string sortBy)
    {
        var specification = new RecipeFilterSpecification(new RecipeQueryParams { SortBy = sortBy });

        Assert.Equal(2, specification.OrderBy.Count);
    }

    [Fact]
    public void Applies_multiple_filters_in_a_single_criteria_expression()
    {
        var categoryId = Guid.NewGuid();
        var specification = new RecipeFilterSpecification(new RecipeQueryParams
        {
            Search = "  couscous ", CategoryId = categoryId, IsTraditional = true,
            Difficulty = DifficultyLevel.Medium.ToString(), Page = 2, PageSize = 500
        });

        Assert.NotNull(specification.Criteria);
        Assert.Equal(100, specification.Take);
        Assert.Equal(100, specification.Skip);
    }
}
