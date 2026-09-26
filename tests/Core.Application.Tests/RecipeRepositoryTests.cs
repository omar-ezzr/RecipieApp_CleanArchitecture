using Core.Application.DTO;
using Core.Application.DTO.Recipe;
using Core.Application.Interfaces.Services;
using Core.Application.Options;
using Core.Application.UseCases.Recipes;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace Core.Application.Tests;

public sealed class RecipeRepositoryTests
{
    [Theory]
    [InlineData(-1, 0, 1, 10, 5, "Recipe 45")]
    [InlineData(0, 500, 1, 100, 1, "Recipe 45")]
    [InlineData(2, 20, 2, 20, 3, "Recipe 25")]
    public async Task GetPagedAsync_returns_metadata_matching_normalized_skip_take(
        int requestedPage,
        int requestedPageSize,
        int expectedPage,
        int expectedPageSize,
        int expectedTotalPages,
        string expectedFirstTitle)
    {
        await using var context = await CreateContextAsync();
        var repository = new RecipeRepository(context);

        var result = await repository.GetPagedAsync(new RecipeQueryParams
        {
            Page = requestedPage,
            PageSize = requestedPageSize
        });

        Assert.Equal(45, result.Total);
        Assert.Equal(expectedPage, result.Page);
        Assert.Equal(expectedPageSize, result.PageSize);
        Assert.Equal(expectedTotalPages, result.TotalPages);
        Assert.Equal(expectedFirstTitle, result.Items.First().Title);
        Assert.True(result.Items.Count <= expectedPageSize);
    }

    [Fact]
    public async Task AddMediaAsync_marks_new_media_added_before_save()
    {
        await using var context = await CreateContextAsync();
        var repository = new RecipeRepository(context);
        var recipe = await context.Recipies.AsNoTracking().FirstAsync();
        var media = new RecipeMedia
        {
            Id = Guid.NewGuid(),
            RecipeId = recipe.Id,
            Url = "/images/recipes/state.jpg",
            ContentType = "image/jpeg",
            MediaType = RecipeMediaType.Image,
            IsMain = true,
            SortOrder = 0
        };

        await repository.AddMediaAsync(media);

        Assert.Equal(EntityState.Added, context.Entry(media).State);
    }

    [Fact]
    public async Task AddMediaAsync_inserts_first_media_and_updates_recipe_cover()
    {
        var commands = new List<string>();
        await using var context = await CreateContextAsync(commands);
        var repository = new RecipeRepository(context);
        var recipe = await context.Recipies.AsNoTracking().FirstAsync();
        var storage = new QueueRecipeMediaStorage("/images/recipes/first.jpg");
        var service = new RecipeService(repository, storage, new RecipeMediaOptions());
        commands.Clear();

        var result = await service.AddMediaAsync(
            recipe.Id,
            new MemoryStream([1, 2, 3]),
            "first.jpg",
            "image/jpeg",
            3,
            recipe.UserId,
            isAdmin: false);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Contains(commands, IsInsertRecipeMedia);
        Assert.DoesNotContain(commands, IsUpdateRecipeMedia);
        var savedRecipe = await context.Recipies
            .AsNoTracking()
            .Include(item => item.Media)
            .SingleAsync(item => item.Id == recipe.Id);
        var media = Assert.Single(savedRecipe.Media);
        Assert.True(media.IsMain);
        Assert.Equal(0, media.SortOrder);
        Assert.Equal("/images/recipes/first.jpg", media.Url);
        Assert.Equal("/images/recipes/first.jpg", savedRecipe.ImageUrl);
        Assert.Empty(storage.DeletedUrls);
    }

    [Fact]
    public async Task AddMediaAsync_inserts_second_media_without_updating_nonexistent_media()
    {
        var commands = new List<string>();
        await using var context = await CreateContextAsync(commands);
        var repository = new RecipeRepository(context);
        var recipe = await context.Recipies.AsNoTracking().FirstAsync();
        var storage = new QueueRecipeMediaStorage("/images/recipes/first.jpg", "/images/recipes/second.mp4");
        var service = new RecipeService(repository, storage, new RecipeMediaOptions());

        var first = await service.AddMediaAsync(
            recipe.Id,
            new MemoryStream([1, 2, 3]),
            "first.jpg",
            "image/jpeg",
            3,
            recipe.UserId,
            isAdmin: false);
        Assert.True(first.IsSuccess, first.Error);
        commands.Clear();

        var second = await service.AddMediaAsync(
            recipe.Id,
            new MemoryStream([4, 5, 6]),
            "second.mp4",
            "video/mp4",
            3,
            recipe.UserId,
            isAdmin: false);

        Assert.True(second.IsSuccess, second.Error);
        Assert.Contains(commands, IsInsertRecipeMedia);
        Assert.DoesNotContain(commands, IsUpdateRecipeMedia);
        var savedRecipe = await context.Recipies
            .AsNoTracking()
            .Include(item => item.Media)
            .SingleAsync(item => item.Id == recipe.Id);
        var orderedMedia = savedRecipe.Media.OrderBy(item => item.SortOrder).ToList();
        Assert.Equal(2, orderedMedia.Count);
        Assert.True(orderedMedia[0].IsMain);
        Assert.False(orderedMedia[1].IsMain);
        Assert.Equal(0, orderedMedia[0].SortOrder);
        Assert.Equal(1, orderedMedia[1].SortOrder);
        Assert.Equal("/images/recipes/first.jpg", savedRecipe.ImageUrl);
        Assert.Empty(storage.DeletedUrls);
    }

    [Fact]
    public async Task Existing_media_operations_mutate_existing_rows()
    {
        var commands = new List<string>();
        await using var context = await CreateContextAsync(commands);
        var repository = new RecipeRepository(context);
        var recipe = await context.Recipies.AsNoTracking().FirstAsync();
        var storage = new QueueRecipeMediaStorage("/images/recipes/first.jpg", "/images/recipes/second.jpg");
        var service = new RecipeService(repository, storage, new RecipeMediaOptions());

        var first = await service.AddMediaAsync(
            recipe.Id,
            new MemoryStream([1, 2, 3]),
            "first.jpg",
            "image/jpeg",
            3,
            recipe.UserId,
            isAdmin: false);
        Assert.True(first.IsSuccess, first.Error);

        var second = await service.AddMediaAsync(
            recipe.Id,
            new MemoryStream([4, 5, 6]),
            "second.jpg",
            "image/jpeg",
            3,
            recipe.UserId,
            isAdmin: false);
        Assert.True(second.IsSuccess, second.Error);

        commands.Clear();
        var setMain = await service.SetMainMediaAsync(recipe.Id, second.Value!.Id, recipe.UserId, isAdmin: false);

        Assert.True(setMain.IsSuccess, setMain.Error);
        Assert.Contains(commands, IsUpdateRecipeMedia);
        Assert.DoesNotContain(commands, IsInsertRecipeMedia);

        commands.Clear();
        var reorder = await service.ReorderMediaAsync(
            recipe.Id,
            [second.Value.Id, first.Value!.Id],
            recipe.UserId,
            isAdmin: false);

        Assert.True(reorder.IsSuccess, reorder.Error);
        Assert.Contains(commands, IsUpdateRecipeMedia);
        Assert.DoesNotContain(commands, IsInsertRecipeMedia);

        commands.Clear();
        var remove = await service.RemoveMediaAsync(recipe.Id, first.Value.Id, recipe.UserId, isAdmin: false);

        Assert.True(remove.IsSuccess, remove.Error);
        Assert.Contains(commands, IsDeleteRecipeMedia);
        Assert.DoesNotContain(commands, IsInsertRecipeMedia);
        Assert.Contains("/images/recipes/first.jpg", storage.DeletedUrls);

        var savedRecipe = await context.Recipies
            .AsNoTracking()
            .Include(item => item.Media)
            .SingleAsync(item => item.Id == recipe.Id);
        var media = Assert.Single(savedRecipe.Media);
        Assert.Equal(second.Value.Id, media.Id);
        Assert.True(media.IsMain);
        Assert.Equal(0, media.SortOrder);
    }

    private static async Task<AppDbContext> CreateContextAsync(List<string>? commands = null)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection);

        if (commands is not null)
        {
            optionsBuilder.AddInterceptors(new CapturingCommandInterceptor(commands));
        }

        var context = new AppDbContext(optionsBuilder.Options);
        await context.Database.EnsureCreatedAsync();

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Dinner"
        };

        context.Categories.Add(category);

        var user = new Users
        {
            Id = Guid.NewGuid(),
            DisplayName = "Owner",
            Email = "owner@example.com",
            PasswordHash = "hash",
            Role = "User",
            IsActive = true
        };

        context.Users.Add(user);

        var cuisine = new Cuisine
        {
            Id = Guid.NewGuid(),
            Name = "Moroccan",
            Slug = "moroccan",
            CountryCode = "MA"
        };

        context.Cuisines.Add(cuisine);

        for (var i = 1; i <= 45; i++)
        {
            context.Recipies.Add(new Recipie
            {
                Id = Guid.NewGuid(),
                Title = $"Recipe {i}",
                Description = "Description",
                PreparationTimeMinutes = i,
                Difficulty = DifficultyLevel.Easy,
                CategoryId = category.Id,
                CuisineId = cuisine.Id,
                UserId = user.Id,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(i)
            });
        }

        await context.SaveChangesAsync();

        return context;
    }

    private static bool IsInsertRecipeMedia(string commandText)
    {
        return commandText.Contains("INSERT INTO \"RecipeMedia\"", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsUpdateRecipeMedia(string commandText)
    {
        return commandText.Contains("UPDATE \"RecipeMedia\"", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDeleteRecipeMedia(string commandText)
    {
        return commandText.Contains("DELETE FROM \"RecipeMedia\"", StringComparison.OrdinalIgnoreCase);
    }

    private sealed class QueueRecipeMediaStorage : IRecipeMediaStorage
    {
        private readonly Queue<string> _urls;

        public QueueRecipeMediaStorage(params string[] urls)
        {
            _urls = new Queue<string>(urls);
        }

        public List<string> DeletedUrls { get; } = [];

        public Task<string> SaveAsync(RecipeMediaUpload upload, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_urls.Dequeue());
        }

        public Task DeleteAsync(string url, CancellationToken cancellationToken = default)
        {
            DeletedUrls.Add(url);
            return Task.CompletedTask;
        }
    }

    private sealed class CapturingCommandInterceptor : DbCommandInterceptor
    {
        private readonly List<string> _commands;

        public CapturingCommandInterceptor(List<string> commands)
        {
            _commands = commands;
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            _commands.Add(command.CommandText);
            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            _commands.Add(command.CommandText);
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result)
        {
            _commands.Add(command.CommandText);
            return base.NonQueryExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            _commands.Add(command.CommandText);
            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }
    }
}
