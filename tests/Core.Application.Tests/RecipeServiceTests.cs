using Core.Application.DTO;
using Core.Application.DTO.Recipe;
using Core.Application.Interfaces;
using Core.Application.UseCases.Recipes;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Application.Tests;

public class RecipeServiceTests
{
    [Theory]
    [InlineData("Easy", DifficultyLevel.Easy)]
    [InlineData("Medium", DifficultyLevel.Medium)]
    [InlineData("Hard", DifficultyLevel.Hard)]
    [InlineData("easy", DifficultyLevel.Easy)]
    public async Task CreateAsync_accepts_valid_difficulty_names(string value, DifficultyLevel expected)
    {
        var repository = new FakeRecipeRepository();
        var service = new RecipeService(repository);

        var result = await service.CreateAsync(NewRecipe(value));

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, repository.AddedRecipe?.Difficulty);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Impossible")]
    public async Task CreateAsync_rejects_missing_or_invalid_difficulty(string value)
    {
        var repository = new FakeRecipeRepository();
        var service = new RecipeService(repository);

        var result = await service.CreateAsync(NewRecipe(value));

        Assert.False(result.IsSuccess);
        Assert.Null(repository.AddedRecipe);
    }

    [Fact]
    public async Task UpdateAsync_preserves_existing_difficulty_when_same_value_is_sent()
    {
        var recipe = ExistingRecipe(DifficultyLevel.Hard);
        var repository = new FakeRecipeRepository { ExistingRecipe = recipe };
        var service = new RecipeService(repository);

        var result = await service.UpdateAsync(recipe.Id, NewRecipe("Hard"));

        Assert.True(result.IsSuccess);
        Assert.Equal(DifficultyLevel.Hard, recipe.Difficulty);
    }

    [Fact]
    public async Task UpdateAsync_changes_difficulty_when_new_value_is_sent()
    {
        var recipe = ExistingRecipe(DifficultyLevel.Hard);
        var repository = new FakeRecipeRepository { ExistingRecipe = recipe };
        var service = new RecipeService(repository);

        var result = await service.UpdateAsync(recipe.Id, NewRecipe("Easy"));

        Assert.True(result.IsSuccess);
        Assert.Equal(DifficultyLevel.Easy, recipe.Difficulty);
    }

    [Fact]
    public async Task GetPagedAsync_returns_normalized_metadata_from_repository()
    {
        var repository = new FakeRecipeRepository
        {
            PagedResult = ([ExistingRecipe(DifficultyLevel.Easy)], 25, 1, 10, 3)
        };
        var service = new RecipeService(repository);

        var result = await service.GetPagedAsync(new RecipeQueryParams { Page = 0, PageSize = 500 });

        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(3, result.TotalPages);
    }

    private static CreateRecipeDto NewRecipe(string difficulty)
    {
        return new CreateRecipeDto
        {
            Title = "Soup",
            Description = "Warm",
            PreparationTimeMinutes = 20,
            CategoryId = Guid.NewGuid(),
            Difficulty = difficulty
        };
    }

    private static Recipie ExistingRecipe(DifficultyLevel difficulty)
    {
        return new Recipie
        {
            Id = Guid.NewGuid(),
            Title = "Soup",
            Description = "Warm",
            PreparationTimeMinutes = 20,
            CategoryId = Guid.NewGuid(),
            Difficulty = difficulty,
            Category = new Category { Id = Guid.NewGuid(), Name = "Dinner" }
        };
    }

    private sealed class FakeRecipeRepository : IRecipeRepository
    {
        public Recipie? ExistingRecipe { get; set; }
        public Recipie? AddedRecipe { get; private set; }
        public (List<Recipie> Items, int Total, int Page, int PageSize, int TotalPages) PagedResult { get; set; } =
            ([], 0, 1, 10, 0);

        public Task<Recipie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExistingRecipe);
        }

        public Task AddAsync(Recipie recipie, CancellationToken cancellationToken = default)
        {
            AddedRecipe = recipie;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Recipie recipie, CancellationToken cancellationToken = default)
        {
            ExistingRecipe = recipie;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Recipie recipie, CancellationToken cancellationToken = default)
        {
            ExistingRecipe = null;
            return Task.CompletedTask;
        }

        public Task<(List<Recipie> Items, int Total, int Page, int PageSize, int TotalPages)> GetPagedAsync(
            RecipeQueryParams parameters,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(PagedResult);
        }
    }
}
