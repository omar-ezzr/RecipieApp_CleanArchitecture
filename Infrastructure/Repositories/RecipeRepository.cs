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

    public async Task<IEnumerable<Recipie>> GetAllAsync()
    {
        return await _context.Recipies
            .Include(r => r.Category)
            .ToListAsync();
    }

    public async Task<Recipie?> GetByIdAsync(Guid id)
    {
        return await _context.Recipies
            .Include(r => r.Category)
            .Include(r => r.Ingredients)
            .Include(r => r.Steps)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task AddAsync(Recipie recipie)
    {
        _context.Recipies.Add(recipie);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Recipie recipie)
    {
        _context.Recipies.Update(recipie);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Recipie recipie)
    {
        _context.Recipies.Remove(recipie);
        await _context.SaveChangesAsync();
    }

   public async Task<(List<Recipie>, int)> GetPagedAsync(RecipeQueryParams parameters)
{
    var page = parameters.Page < 1 ? 1 : parameters.Page;
    var pageSize = parameters.PageSize < 1 ? 10 : parameters.PageSize;
    pageSize = Math.Min(pageSize, 1000);

    var query = _context.Recipies
        .Include(r => r.Category)
        .Include(r => r.Ingredients)
        .Include(r => r.Steps)
        .AsQueryable();

    if (!string.IsNullOrEmpty(parameters.Search))
    {
        var search = parameters.Search.ToLower();
        query = query.Where(r => r.Title.ToLower().Contains(search));
    }

    if (!string.IsNullOrEmpty(parameters.Difficulty) &&
        Enum.TryParse<DifficultyLevel>(parameters.Difficulty, true, out var difficulty))
    {
        query = query.Where(r => r.Difficulty == difficulty);
    }

    if (parameters.CategoryId.HasValue)
    {
        query = query.Where(r => r.CategoryId == parameters.CategoryId.Value);
    }

    query = parameters.SortBy?.ToLower() switch
    {
        "title" => query.OrderBy(r => r.Title),
        _ => query.OrderByDescending(r => r.CreatedAt)
    };

    var total = await query.CountAsync();

    var data = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return (data, total);
}
}

