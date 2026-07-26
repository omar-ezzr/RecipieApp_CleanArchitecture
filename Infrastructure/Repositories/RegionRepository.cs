using Core.Application.DTO.Regions;
using Core.Application.Interfaces.Repositories;
using Core.Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class RegionRepository : IRegionRepository
{
    private readonly AppDbContext _context;

    public RegionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RegionDto>> GetAllByCuisineAsync(Guid cuisineId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.Regions
            .AsNoTracking()
            .Where(region => region.CuisineId == cuisineId);

        if (activeOnly)
        {
            query = query.Where(region => region.IsActive);
        }

        return await query
            .OrderBy(region => region.Name)
            .Select(region => new RegionDto
            {
                Id = region.Id,
                Name = region.Name,
                Slug = region.Slug,
                Description = region.Description,
                CuisineId = region.CuisineId,
                CuisineName = region.Cuisine.Name,
                ImageUrl = region.ImageUrl,
                IsActive = region.IsActive,
                CreatedAt = region.CreatedAt,
                RecipeCount = region.Recipes.Count()
            })
            .ToListAsync(cancellationToken);
    }

    public Task<Region?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Regions
            .Include(region => region.Cuisine)
            .FirstOrDefaultAsync(region => region.Id == id, cancellationToken);
    }

    public Task<Region?> GetBySlugAsync(Guid cuisineId, string slug, CancellationToken cancellationToken = default)
    {
        return _context.Regions
            .Include(region => region.Cuisine)
            .FirstOrDefaultAsync(region => region.CuisineId == cuisineId && region.Slug == slug, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(Guid cuisineId, string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLowerInvariant();

        return _context.Regions
            .AsNoTracking()
            .AnyAsync(region => region.CuisineId == cuisineId
                && region.Name.ToLower() == normalized
                && (!excludeId.HasValue || region.Id != excludeId.Value), cancellationToken);
    }

    public Task<bool> ExistsBySlugInCuisineAsync(Guid cuisineId, string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return _context.Regions
            .AsNoTracking()
            .AnyAsync(region => region.CuisineId == cuisineId
                && region.Slug == slug
                && (!excludeId.HasValue || region.Id != excludeId.Value), cancellationToken);
    }

    public async Task AddAsync(Region region, CancellationToken cancellationToken = default)
    {
        await _context.Regions.AddAsync(region, cancellationToken);
    }

    public void Update(Region region)
    {
        _context.Regions.Update(region);
    }

    public void Delete(Region region)
    {
        _context.Regions.Remove(region);
    }

    public Task<bool> HasRecipesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Recipies.AsNoTracking().AnyAsync(recipe => recipe.RegionId == id, cancellationToken);
    }

    public Task<int> CountRecipesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Recipies.AsNoTracking().CountAsync(recipe => recipe.RegionId == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
