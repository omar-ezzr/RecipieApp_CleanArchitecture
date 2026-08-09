using Core.Application.Common;
using Core.Application.DTO.Users;
using Core.Application.Interfaces.Repositories;
using Core.Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class UserFollowRepository : IUserFollowRepository
{
    private readonly AppDbContext _context;

    public UserFollowRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ActiveUserExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AsNoTracking().AnyAsync(user => user.Id == userId && user.IsActive, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid followerUserId, Guid followedUserId, CancellationToken cancellationToken = default)
    {
        return await _context.UserFollows
            .AsNoTracking()
            .AnyAsync(follow => follow.FollowerUserId == followerUserId && follow.FollowedUserId == followedUserId, cancellationToken);
    }

    public async Task AddAsync(UserFollow follow, CancellationToken cancellationToken = default)
    {
        await _context.UserFollows.AddAsync(follow, cancellationToken);
    }

    public async Task RemoveAsync(Guid followerUserId, Guid followedUserId, CancellationToken cancellationToken = default)
    {
        var follow = await _context.UserFollows.FirstOrDefaultAsync(
            item => item.FollowerUserId == followerUserId && item.FollowedUserId == followedUserId,
            cancellationToken);

        if (follow is not null)
        {
            _context.UserFollows.Remove(follow);
        }
    }

    public Task<PagedResult<UserSummaryDto>> GetFollowersAsync(Guid userId, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.UserFollows
            .AsNoTracking()
            .Where(follow => follow.FollowedUserId == userId && follow.FollowerUser.IsActive)
            .OrderByDescending(follow => follow.CreatedAt)
            .ThenByDescending(follow => follow.Id)
            .Select(follow => follow.FollowerUser);

        return ProjectUsersAsync(query, currentUserId, page, pageSize, cancellationToken);
    }

    public Task<PagedResult<UserSummaryDto>> GetFollowingAsync(Guid userId, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.UserFollows
            .AsNoTracking()
            .Where(follow => follow.FollowerUserId == userId && follow.FollowedUser.IsActive)
            .OrderByDescending(follow => follow.CreatedAt)
            .ThenByDescending(follow => follow.Id)
            .Select(follow => follow.FollowedUser);

        return ProjectUsersAsync(query, currentUserId, page, pageSize, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<PagedResult<UserSummaryDto>> ProjectUsersAsync(
        IQueryable<Users> query,
        Guid? currentUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 50);
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
}
