using Core.Application.DTO;
using Core.Application.DTO.Recipe;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Services;
using Core.Application.Options;
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

    [Fact]
    public async Task AddMediaAsync_makes_only_the_first_photo_main_and_updates_cover()
    {
        var recipe = ExistingRecipe(DifficultyLevel.Easy);
        var storage = new FakeRecipeMediaStorage("/images/recipes/one.jpg", "/images/recipes/two.png");
        var service = new RecipeService(new FakeRecipeRepository { ExistingRecipe = recipe }, storage, new RecipeMediaOptions());

        var first = await service.AddMediaAsync(recipe.Id, new MemoryStream([1]), "one.jpg", "image/jpeg", 1, UserId, false);
        var second = await service.AddMediaAsync(recipe.Id, new MemoryStream([2]), "two.png", "image/png", 1, UserId, false);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(2, recipe.Media.Count);
        Assert.True(recipe.Media.Single(media => media.Id == first.Value!.Id).IsMain);
        Assert.False(recipe.Media.Single(media => media.Id == second.Value!.Id).IsMain);
        Assert.Equal("/images/recipes/one.jpg", recipe.ImageUrl);
        Assert.All(recipe.Media, media => Assert.Equal(RecipeMediaType.Image, media.MediaType));
    }

    [Fact]
    public async Task Photo_mutations_enforce_owner_admin_and_final_photo_rules()
    {
        var recipe = ExistingRecipe(DifficultyLevel.Easy);
        var firstPhoto = Photo("one.jpg", true, 0);
        var secondPhoto = Photo("two.jpg", false, 1);
        recipe.Media = [firstPhoto, secondPhoto];
        recipe.ImageUrl = firstPhoto.Url;
        var storage = new FakeRecipeMediaStorage();
        var service = new RecipeService(new FakeRecipeRepository { ExistingRecipe = recipe }, storage, new RecipeMediaOptions());

        var forbidden = await service.SetMainMediaAsync(recipe.Id, secondPhoto.Id, OtherUserId, false);
        var admin = await service.SetMainMediaAsync(recipe.Id, secondPhoto.Id, OtherUserId, true);
        var removeMain = await service.RemoveMediaAsync(recipe.Id, secondPhoto.Id, UserId, false);
        var finalPhoto = await service.RemoveMediaAsync(recipe.Id, firstPhoto.Id, UserId, false);

        Assert.Equal(ServiceErrorType.Forbidden, forbidden.ErrorType);
        Assert.True(admin.IsSuccess);
        Assert.True(removeMain.IsSuccess);
        Assert.Equal("/images/recipes/one.jpg", recipe.ImageUrl);
        Assert.Equal(ServiceErrorType.Validation, finalPhoto.ErrorType);
        Assert.Contains("final photo", finalPhoto.Error!);
        Assert.Contains("/images/recipes/two.jpg", storage.DeletedUrls);
    }

    [Fact]
    public async Task ReorderMediaAsync_rejects_duplicate_and_missing_ids_then_normalizes_sort_order()
    {
        var recipe = ExistingRecipe(DifficultyLevel.Easy);
        var firstPhoto = Photo("one.jpg", true, 7);
        var secondPhoto = Photo("two.jpg", false, 4);
        var thirdPhoto = Photo("three.jpg", false, 9);
        recipe.Media = [firstPhoto, secondPhoto, thirdPhoto];
        var service = new RecipeService(new FakeRecipeRepository { ExistingRecipe = recipe }, new FakeRecipeMediaStorage());

        var duplicate = await service.ReorderMediaAsync(recipe.Id, [firstPhoto.Id, firstPhoto.Id, thirdPhoto.Id], UserId, false);
        var missing = await service.ReorderMediaAsync(recipe.Id, [firstPhoto.Id, secondPhoto.Id, Guid.NewGuid()], UserId, false);
        var reordered = await service.ReorderMediaAsync(recipe.Id, [thirdPhoto.Id, secondPhoto.Id, firstPhoto.Id], UserId, false);

        Assert.Equal(ServiceErrorType.Validation, duplicate.ErrorType);
        Assert.Equal(ServiceErrorType.Validation, missing.ErrorType);
        Assert.True(reordered.IsSuccess);
        Assert.Equal([0, 1, 2], recipe.Media.OrderBy(media => media.SortOrder).Select(media => media.SortOrder));
        Assert.Equal("/images/recipes/one.jpg", recipe.ImageUrl);
    }

    [Fact]
    public async Task AddMediaAsync_rejects_tenth_photo()
    {
        var recipe = ExistingRecipe(DifficultyLevel.Easy);
        recipe.Media = Enumerable.Range(0, 9).Select(index => Photo($"{index}.jpg", index == 0, index)).ToList();
        var storage = new FakeRecipeMediaStorage("/images/recipes/ten.jpg");
        var service = new RecipeService(new FakeRecipeRepository { ExistingRecipe = recipe }, storage, new RecipeMediaOptions());

        var result = await service.AddMediaAsync(recipe.Id, new MemoryStream([1]), "ten.jpg", "image/jpeg", 1, UserId, false);

        Assert.Equal(ServiceErrorType.Validation, result.ErrorType);
        Assert.Empty(storage.SavedUploads);
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

    private static RecipeMedia Photo(string fileName, bool isMain, int sortOrder) => new()
    {
        Id = Guid.NewGuid(), Url = $"/images/recipes/{fileName}", ContentType = "image/jpeg",
        MediaType = RecipeMediaType.Image, IsMain = isMain, SortOrder = sortOrder, CreatedAt = DateTime.UtcNow
    };

    private sealed class FakeRecipeMediaStorage(params string[] urls) : IRecipeMediaStorage
    {
        private readonly Queue<string> _urls = new(urls);
        public List<string> DeletedUrls { get; } = [];
        public List<RecipeMediaUpload> SavedUploads { get; } = [];

        public Task<string> SaveAsync(RecipeMediaUpload upload, CancellationToken cancellationToken = default)
        {
            SavedUploads.Add(upload);
            return Task.FromResult(_urls.Dequeue());
        }

        public Task DeleteAsync(string url, CancellationToken cancellationToken = default)
        {
            DeletedUrls.Add(url);
            return Task.CompletedTask;
        }
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

        public Task AddMediaAsync(RecipeMedia media, CancellationToken cancellationToken = default)
        {
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

        public Task<IReadOnlyDictionary<Guid, RecipeLikeStatsDto>> GetLikeStatsAsync(
            IReadOnlyCollection<Guid> recipeIds,
            Guid? currentUserId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyDictionary<Guid, RecipeLikeStatsDto> result = recipeIds.ToDictionary(
                id => id,
                id => new RecipeLikeStatsDto
                {
                    RecipeId = id,
                    LikeCount = 0,
                    IsLikedByCurrentUser = false
                });

            return Task.FromResult(result);
        }
    }
}
