using Core.Application.DTO.Categories;
using Core.Domain.Entities;
using Core.Application.Interfaces.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            })
            .ToListAsync(cancellationToken);
    }
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => _context.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<bool> IsUsedAsync(Guid id, CancellationToken cancellationToken = default) => _context.Recipies.AsNoTracking().AnyAsync(x => x.CategoryId == id, cancellationToken);
    public async Task AddAsync(Category category, CancellationToken cancellationToken = default) { _context.Categories.Add(category); await _context.SaveChangesAsync(cancellationToken); }
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
    public async Task DeleteAsync(Category category, CancellationToken cancellationToken = default) { _context.Categories.Remove(category); await _context.SaveChangesAsync(cancellationToken); }
}
