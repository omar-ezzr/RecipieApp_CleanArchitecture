using Core.Application.Common;
using Core.Application.DTO.Social;
using Core.Application.DTO.Users;
using Core.Application.Interfaces.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class FeedRepository : IFeedRepository
{
    private readonly AppDbContext _context;

    public FeedRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<FeedRecipeDto>> GetFollowingFeedAsync(
        Guid currentUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 50);

        var followedUserIds = _context.UserFollows
            .Where(follow => follow.FollowerUserId == currentUserId)
            .Select(follow => follow.FollowedUserId);

        var query = _context.Recipies
            .AsNoTracking()
            .Where(recipe => followedUserIds.Contains(recipe.UserId) && recipe.User.IsActive)
            .OrderByDescending(recipe => recipe.CreatedAt)
            .ThenByDescending(recipe => recipe.Id);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(recipe => new FeedRecipeDto
            {
                Id = recipe.Id,
                Title = recipe.Title,
                Description = recipe.Description,
                ImageUrl = recipe.ImageUrl,
                PreparationTimeMinutes = recipe.PreparationTimeMinutes,
                Difficulty = recipe.Difficulty,
                CreatedAt = recipe.CreatedAt,
                Author = new AuthorDto
                {
                    Id = recipe.UserId,
                    DisplayName = recipe.User.DisplayName,
                    AvatarUrl = recipe.User.AvatarUrl
                },
                Cuisine = new NamedSummaryDto
                {
                    Id = recipe.CuisineId,
                    Name = recipe.Cuisine.Name
                },
                Region = recipe.Region == null ? null : new NamedSummaryDto
                {
                    Id = recipe.Region.Id,
                    Name = recipe.Region.Name
                },
                IsTraditional = recipe.IsTraditional,
                LikeCount = _context.RecipeLikes.Count(like => like.RecipeId == recipe.Id),
                CommentCount = _context.RecipeComments.Count(comment => comment.RecipeId == recipe.Id),
                IsLikedByCurrentUser = _context.RecipeLikes.Any(like => like.RecipeId == recipe.Id && like.UserId == currentUserId),
                IsFavoriteByCurrentUser = _context.FavoriteRecipes.Any(favorite => favorite.RecipeId == recipe.Id && favorite.UserId == currentUserId)
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<FeedRecipeDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize)
        };
    }
}
