using Core.Application.Common;
using Core.Application.DTO.Users;
using Core.Application.Interfaces.Repositories;
using Core.Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class RecipeLikeRepository : IRecipeLikeRepository
{
    private readonly AppDbContext _context;

    public RecipeLikeRepository(AppDbContext context)
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

    public async Task<bool> ExistsAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default)
    {
        return await _context.RecipeLikes
            .AsNoTracking()
            .AnyAsync(like => like.UserId == userId && like.RecipeId == recipeId, cancellationToken);
    }

    public async Task<int> CountAsync(Guid recipeId, CancellationToken cancellationToken = default)
    {
        return await _context.RecipeLikes
            .AsNoTracking()
            .CountAsync(like => like.RecipeId == recipeId, cancellationToken);
    }

    public async Task AddAsync(RecipeLike like, CancellationToken cancellationToken = default)
    {
        await _context.RecipeLikes.AddAsync(like, cancellationToken);
    }

    public async Task RemoveAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default)
    {
        var like = await _context.RecipeLikes
            .FirstOrDefaultAsync(item => item.UserId == userId && item.RecipeId == recipeId, cancellationToken);

        if (like is not null)
        {
            _context.RecipeLikes.Remove(like);
        }
    }

    public async Task<PagedResult<UserSummaryDto>> GetRecipeLikesAsync(
        Guid recipeId,
        Guid? currentUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 50);

        var query = _context.RecipeLikes
            .AsNoTracking()
            .Where(like => like.RecipeId == recipeId && like.User.IsActive)
            .OrderByDescending(like => like.CreatedAt)
            .ThenByDescending(like => like.Id)
            .Select(like => like.User);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new UserSummaryDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                AvatarUrl = user.AvatarUrl,
                CountryCode = user.CountryCode,
                IsFollowing = currentUserId.HasValue && _context.UserFollows.Any(follow =>
                    follow.FollowerUserId == currentUserId.Value && follow.FollowedUserId == user.Id)
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<UserSummaryDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
