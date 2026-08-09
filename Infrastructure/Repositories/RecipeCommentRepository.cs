using Core.Application.Common;
using Core.Application.DTO.Social;
using Core.Application.DTO.Users;
using Core.Application.Interfaces.Repositories;
using Core.Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class RecipeCommentRepository : IRecipeCommentRepository
{
    private readonly AppDbContext _context;

    public RecipeCommentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Exists, Guid? OwnerId)> GetRecipeStatusAsync(Guid recipeId, CancellationToken cancellationToken = default)
    {
        var ownerId = await _context.Recipies
            .AsNoTracking()
            .Where(recipe => recipe.Id == recipeId)
            .Select(recipe => (Guid?)recipe.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        return (ownerId.HasValue, ownerId);
    }

    public async Task<RecipeComment?> GetByIdAsync(Guid commentId, bool track = false, CancellationToken cancellationToken = default)
    {
        var query = track ? _context.RecipeComments.AsQueryable() : _context.RecipeComments.AsNoTracking();
        return await query.FirstOrDefaultAsync(comment => comment.Id == commentId, cancellationToken);
    }

    public async Task AddAsync(RecipeComment comment, CancellationToken cancellationToken = default)
    {
        await _context.RecipeComments.AddAsync(comment, cancellationToken);
    }

    public Task DeleteAsync(RecipeComment comment)
    {
        _context.RecipeComments.Remove(comment);
        return Task.CompletedTask;
    }

    public async Task<PagedResult<RecipeCommentDto>> GetByRecipeAsync(Guid recipeId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 50);

        var query = _context.RecipeComments
            .AsNoTracking()
            .Where(comment => comment.RecipeId == recipeId)
            .OrderByDescending(comment => comment.CreatedAt)
            .ThenByDescending(comment => comment.Id);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(comment => new RecipeCommentDto
            {
                Id = comment.Id,
                RecipeId = comment.RecipeId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                Author = new AuthorDto
                {
                    Id = comment.UserId,
                    DisplayName = comment.User.DisplayName,
                    AvatarUrl = comment.User.AvatarUrl
                }
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<RecipeCommentDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task<RecipeCommentDto?> GetDtoByIdAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        return await _context.RecipeComments
            .AsNoTracking()
            .Where(comment => comment.Id == commentId)
            .Select(comment => new RecipeCommentDto
            {
                Id = comment.Id,
                RecipeId = comment.RecipeId,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                Author = new AuthorDto
                {
                    Id = comment.UserId,
                    DisplayName = comment.User.DisplayName,
                    AvatarUrl = comment.User.AvatarUrl
                }
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
