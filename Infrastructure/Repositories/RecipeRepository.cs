using Core.Application.DTO;
using Core.Application.DTO.Recipe;
using Core.Application.Interfaces;
using Core.Application.Specifications.Recipes;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class RecipeRepository : IRecipeRepository
{
    private readonly AppDbContext _context;

    public RecipeRepository(AppDbContext context)
    {
        _context = context;
    }

    // ========================
    // GET ALL
    // ========================
    public async Task<Recipie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Recipies
            .Include(r => r.User)
            .Include(r => r.Category)
            .Include(r => r.Cuisine)
            .Include(r => r.Region)
            .Include(r => r.Ingredients)
            .Include(r => r.Steps)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .AnyAsync(category => category.Id == categoryId, cancellationToken);
    }

    public async Task<bool> CuisineExistsAsync(Guid cuisineId, CancellationToken cancellationToken = default)
    {
        return await _context.Cuisines
            .AsNoTracking()
            .AnyAsync(cuisine => cuisine.Id == cuisineId && cuisine.IsActive, cancellationToken);
    }

    public async Task<Region?> GetActiveRegionAsync(Guid regionId, CancellationToken cancellationToken = default)
    {
        return await _context.Regions
            .AsNoTracking()
            .FirstOrDefaultAsync(region => region.Id == regionId && region.IsActive, cancellationToken);
    }

    // ========================
    // CREATE
    // ========================
    public async Task AddAsync(Recipie recipie, CancellationToken cancellationToken = default)
    {
        _context.Recipies.Add(recipie);
        await _context.SaveChangesAsync(cancellationToken);
    }

    // ========================
    // UPDATE
    // ========================
    public async Task UpdateAsync(Recipie recipie, CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    // ========================
    // DELETE
    // ========================
    public async Task DeleteAsync(Recipie recipie, CancellationToken cancellationToken = default)
    {
        _context.Recipies.Remove(recipie);
        await _context.SaveChangesAsync(cancellationToken);
    }

    // ========================
    // PAGINATION + FILTERING
    // ========================
    public async Task<(List<Recipie> Items, int Total, int Page, int PageSize, int TotalPages)> GetPagedAsync(
        RecipeQueryParams parameters,
        CancellationToken cancellationToken = default)
    {
        var specification = new RecipeFilterSpecification(parameters);
        var countSpecification = new RecipeFilterSpecification(parameters, applyPaging: false);
        var baseQuery = _context.Recipies.AsNoTracking();
        var total = await SpecificationEvaluator.GetQuery(baseQuery, countSpecification).CountAsync(cancellationToken);
        var data = await SpecificationEvaluator.GetQuery(baseQuery, specification)
            .Include(recipe => recipe.User)
            .Include(recipe => recipe.Category)
            .Include(recipe => recipe.Cuisine)
            .Include(recipe => recipe.Region)
            .ToListAsync(cancellationToken);
        var page = Math.Max(1, parameters.Page);
        var pageSize = Math.Min(100, parameters.PageSize < 1 ? 10 : parameters.PageSize);
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return (data, total, page, pageSize, totalPages);
    }

    public async Task<IReadOnlyDictionary<Guid, RecipeLikeStatsDto>> GetLikeStatsAsync(
        IReadOnlyCollection<Guid> recipeIds,
        Guid? currentUserId,
        CancellationToken cancellationToken = default)
    {
        if (recipeIds.Count == 0)
        {
            return new Dictionary<Guid, RecipeLikeStatsDto>();
        }

        var ids = recipeIds.Distinct().ToList();

        var likeCounts = await _context.RecipeLikes
            .AsNoTracking()
            .Where(like => ids.Contains(like.RecipeId))
            .GroupBy(like => like.RecipeId)
            .Select(group => new
            {
                RecipeId = group.Key,
                Count = group.Count()
            })
            .ToDictionaryAsync(item => item.RecipeId, item => item.Count, cancellationToken);

        var likedRecipeIds = currentUserId.HasValue
            ? await _context.RecipeLikes
                .AsNoTracking()
                .Where(like => ids.Contains(like.RecipeId) && like.UserId == currentUserId.Value)
                .Select(like => like.RecipeId)
                .ToListAsync(cancellationToken)
            : [];

        var likedSet = likedRecipeIds.ToHashSet();

        return ids.ToDictionary(
            id => id,
            id => new RecipeLikeStatsDto
            {
                RecipeId = id,
                LikeCount = likeCounts.GetValueOrDefault(id),
                IsLikedByCurrentUser = likedSet.Contains(id)
            });
    }
}
