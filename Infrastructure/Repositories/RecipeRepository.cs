using Core.Application.DTO;
using Core.Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Infrastructure.Persistence;
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
        var page = parameters.Page < 1 ? 1 : parameters.Page;
        var pageSize = parameters.PageSize < 1 ? 10 : parameters.PageSize;
        pageSize = Math.Min(pageSize, 100); // protect server

        var query = _context.Recipies
            .Include(r => r.User)
            .Include(r => r.Category)
            .Include(r => r.Cuisine)
            .Include(r => r.Region)
            .AsNoTracking()
            .AsQueryable();

        // ========================
        // SEARCH
        // ========================
        if (!string.IsNullOrEmpty(parameters.Search))
        {
            query = query.Where(r => r.Title.Contains(parameters.Search));
        }

        // ========================
        // CATEGORY FILTER (safe)
        // ========================
        if (parameters.CategoryId.HasValue)
{
    query = query.Where(r => r.CategoryId == parameters.CategoryId.Value);
}

        if (parameters.UserId.HasValue)
        {
            query = query.Where(r => r.UserId == parameters.UserId.Value);
        }

        if (parameters.CuisineId.HasValue)
        {
            query = query.Where(r => r.CuisineId == parameters.CuisineId.Value);
        }

        if (parameters.RegionId.HasValue)
        {
            query = query.Where(r => r.RegionId == parameters.RegionId.Value);
        }

        if (parameters.IsTraditional.HasValue)
        {
            query = query.Where(r => r.IsTraditional == parameters.IsTraditional.Value);
        }

        // ========================
        // DIFFICULTY FILTER (safe)
        // ========================
        if (!string.IsNullOrWhiteSpace(parameters.Difficulty) &&
            Enum.TryParse<DifficultyLevel>(parameters.Difficulty, true, out var difficulty) &&
            Enum.IsDefined(typeof(DifficultyLevel), difficulty))
        {
            query = query.Where(r => r.Difficulty == difficulty);
        }

        // ========================
        // SORTING
        // ========================
        query = parameters.SortBy?.ToLower() switch
        {
            "title" => query.OrderBy(r => r.Title),
            "time" => query.OrderBy(r => r.PreparationTimeMinutes),
            "difficulty" => query.OrderBy(r => r.Difficulty),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };

        // ========================
        // COUNT
        // ========================
        var total = await query.CountAsync(cancellationToken);

        // ========================
        // PAGINATION
        // ========================
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

        return (data, total, page, pageSize, totalPages);
    }
}
