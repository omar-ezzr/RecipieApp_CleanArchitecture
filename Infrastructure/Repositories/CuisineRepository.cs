using Core.Application.DTO.Cuisines;
using Core.Application.Interfaces.Repositories;
using Core.Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class CuisineRepository : ICuisineRepository
{
    private readonly AppDbContext _context;

    public CuisineRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CuisineDto>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.Cuisines.AsNoTracking();

        if (activeOnly)
        {
            query = query.Where(cuisine => cuisine.IsActive);
        }

        return await query
            .OrderBy(cuisine => cuisine.Name)
            .Select(cuisine => new CuisineDto
            {
                Id = cuisine.Id,
                Name = cuisine.Name,
                Slug = cuisine.Slug,
                Description = cuisine.Description,
                CountryCode = cuisine.CountryCode,
                ImageUrl = cuisine.ImageUrl,
                IsActive = cuisine.IsActive,
                CreatedAt = cuisine.CreatedAt,
                RegionCount = cuisine.Regions.Count(region => region.IsActive),
                RecipeCount = cuisine.Recipes.Count()
            })
            .ToListAsync(cancellationToken);
    }

    public Task<Cuisine?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Cuisines.FirstOrDefaultAsync(cuisine => cuisine.Id == id, cancellationToken);
    }

    public Task<Cuisine?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return _context.Cuisines.FirstOrDefaultAsync(cuisine => cuisine.Slug == slug, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLowerInvariant();

        return _context.Cuisines
            .AsNoTracking()
            .AnyAsync(cuisine => cuisine.Name.ToLower() == normalized && (!excludeId.HasValue || cuisine.Id != excludeId.Value), cancellationToken);
    }

    public Task<bool> ExistsBySlugAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return _context.Cuisines
            .AsNoTracking()
            .AnyAsync(cuisine => cuisine.Slug == slug && (!excludeId.HasValue || cuisine.Id != excludeId.Value), cancellationToken);
    }

    public async Task AddAsync(Cuisine cuisine, CancellationToken cancellationToken = default)
    {
        await _context.Cuisines.AddAsync(cuisine, cancellationToken);
    }

    public void Update(Cuisine cuisine)
    {
        _context.Cuisines.Update(cuisine);
    }

    public void Delete(Cuisine cuisine)
    {
        _context.Cuisines.Remove(cuisine);
    }

    public Task<bool> HasRecipesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Recipies.AsNoTracking().AnyAsync(recipe => recipe.CuisineId == id, cancellationToken);
    }

    public Task<bool> HasRegionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Regions.AsNoTracking().AnyAsync(region => region.CuisineId == id, cancellationToken);
    }

    public Task<int> CountRecipesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Recipies.AsNoTracking().CountAsync(recipe => recipe.CuisineId == id, cancellationToken);
    }

    public Task<int> CountRegionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Regions.AsNoTracking().CountAsync(region => region.CuisineId == id && region.IsActive, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
