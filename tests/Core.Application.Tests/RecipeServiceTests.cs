using Core.Application.DTO;
using Core.Application.DTO.Recipe;
using Core.Application.Interfaces;
using Core.Application.Common;
using Core.Application.UseCases.Recipes;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Application.Tests;

public class RecipeServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();
    private static readonly Guid CuisineId = Guid.NewGuid();

    [Theory]
    [InlineData(DifficultyLevel.Easy)]
    [InlineData(DifficultyLevel.Medium)]
    [InlineData(DifficultyLevel.Hard)]
    public async Task CreateAsync_accepts_valid_difficulty_values(DifficultyLevel value)
    {
        var repository = new FakeRecipeRepository();
        var service = new RecipeService(repository);

        var result = await service.CreateAsync(NewRecipe(value), UserId);

        Assert.True(result.IsSuccess);
        Assert.Equal(value, repository.AddedRecipe?.Difficulty);
        Assert.Equal(UserId, repository.AddedRecipe?.UserId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(999)]
    public async Task CreateAsync_rejects_missing_or_invalid_difficulty(int value)
    {
        var repository = new FakeRecipeRepository();
        var service = new RecipeService(repository);

        var result = await service.CreateAsync(NewRecipe((DifficultyLevel)value), UserId);

        Assert.False(result.IsSuccess);
        Assert.Null(repository.AddedRecipe);
    }

    [Fact]
    public async Task UpdateAsync_preserves_existing_difficulty_when_same_value_is_sent()
    {
        var recipe = ExistingRecipe(DifficultyLevel.Hard);
        var repository = new FakeRecipeRepository { ExistingRecipe = recipe };
        var service = new RecipeService(repository);

        var result = await service.UpdateAsync(recipe.Id, NewRecipe(DifficultyLevel.Hard), recipe.UserId, isAdmin: false);

        Assert.True(result.IsSuccess);
        Assert.Equal(DifficultyLevel.Hard, recipe.Difficulty);
    }

    [Fact]
    public async Task UpdateAsync_changes_difficulty_when_new_value_is_sent()
    {
        var recipe = ExistingRecipe(DifficultyLevel.Hard);
        var repository = new FakeRecipeRepository { ExistingRecipe = recipe };
        var service = new RecipeService(repository);

        var result = await service.UpdateAsync(recipe.Id, NewRecipe(DifficultyLevel.Easy), recipe.UserId, isAdmin: false);

        Assert.True(result.IsSuccess);
        Assert.Equal(DifficultyLevel.Easy, recipe.Difficulty);
    }

    [Fact]
    public async Task UpdateAsync_rejects_non_owner_when_not_admin()
    {
        var recipe = ExistingRecipe(DifficultyLevel.Hard);
        var repository = new FakeRecipeRepository { ExistingRecipe = recipe };
        var service = new RecipeService(repository);

        var result = await service.UpdateAsync(recipe.Id, NewRecipe(DifficultyLevel.Easy), OtherUserId, isAdmin: false);

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.Forbidden, result.ErrorType);
    }

    [Fact]
    public async Task UpdateAsync_allows_admin_for_any_recipe()
    {
        var recipe = ExistingRecipe(DifficultyLevel.Hard);
        var repository = new FakeRecipeRepository { ExistingRecipe = recipe };
        var service = new RecipeService(repository);

        var result = await service.UpdateAsync(recipe.Id, NewRecipe(DifficultyLevel.Easy), OtherUserId, isAdmin: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(DifficultyLevel.Easy, recipe.Difficulty);
    }

    [Fact]
    public async Task DeleteAsync_rejects_non_owner_when_not_admin()
    {
        var recipe = ExistingRecipe(DifficultyLevel.Hard);
        var repository = new FakeRecipeRepository { ExistingRecipe = recipe };
        var service = new RecipeService(repository);

        var result = await service.DeleteAsync(recipe.Id, OtherUserId, isAdmin: false);

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.Forbidden, result.ErrorType);
        Assert.NotNull(repository.ExistingRecipe);
    }

    [Fact]
    public async Task DeleteAsync_allows_owner()
    {
        var recipe = ExistingRecipe(DifficultyLevel.Hard);
        var repository = new FakeRecipeRepository { ExistingRecipe = recipe };
        var service = new RecipeService(repository);

        var result = await service.DeleteAsync(recipe.Id, recipe.UserId, isAdmin: false);

        Assert.True(result.IsSuccess);
        Assert.Null(repository.ExistingRecipe);
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

    private static CreateRecipeDto NewRecipe(DifficultyLevel difficulty)
    {
        return new CreateRecipeDto
        {
            Title = "Soup",
            Description = "Warm",
            PreparationTimeMinutes = 20,
            CategoryId = Guid.NewGuid(),
            CuisineId = CuisineId,
            Difficulty = difficulty,
            Ingredients = [new CreateIngredientDto { Name = "Salt", Quantity = "1 tsp" }],
            Steps = [new CreateRecipeStepDto { StepNumber = 1, Instruction = "Cook" }]
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
            CuisineId = CuisineId,
            Difficulty = difficulty,
            Category = new Category { Id = Guid.NewGuid(), Name = "Dinner" },
            Cuisine = new Cuisine { Id = CuisineId, Name = "Moroccan", Slug = "moroccan", CountryCode = "MA" },
            UserId = UserId,
            User = new Users { Id = UserId, DisplayName = "Owner", Email = "owner@example.com" }
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

        public Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<bool> CuisineExistsAsync(Guid cuisineId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<Region?> GetActiveRegionAsync(Guid regionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Region?>(new Region
            {
                Id = regionId,
                Name = "Souss-Massa",
                Slug = "souss-massa",
                CuisineId = CuisineId,
                Cuisine = new Cuisine { Id = CuisineId, Name = "Moroccan", Slug = "moroccan", CountryCode = "MA" }
            });
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
