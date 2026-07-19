using Core.Application.DTO;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

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

    private static async Task<AppDbContext> CreateContextAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Dinner"
        };

        context.Categories.Add(category);

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
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(i)
            });
        }

        await context.SaveChangesAsync();

        return context;
    }
}
