using Core.Application.DTO.Users;
using Core.Application.Interfaces.Repositories;
using Core.Domain.Constants;
using Core.Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyCollection<Users> Items, int Total, int Page, int PageSize)> GetPagedAsync(
        UserQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var page = parameters.Page < 1 ? 1 : parameters.Page;
        var pageSize = parameters.PageSize < 1 ? 20 : Math.Min(parameters.PageSize, 100);

        var query = _context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim().ToLowerInvariant();
            query = query.Where(user => user.Email.Contains(search));
        }

        if (AppRoles.TryNormalize(parameters.Role, out var role))
        {
            query = query.Where(user => user.Role == role);
        }

        if (parameters.IsActive.HasValue)
        {
            query = query.Where(user => user.IsActive == parameters.IsActive.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(user => user.Email)
            .ThenBy(user => user.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total, page, pageSize);
    }

    public async Task<Users?> GetByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default)
    {
        var query = track ? _context.Users.AsQueryable() : _context.Users.AsNoTracking();

        return await query.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task<Users?> GetByEmailAsync(string normalizedEmail, bool track = false, CancellationToken cancellationToken = default)
    {
        var query = track ? _context.Users.AsQueryable() : _context.Users.AsNoTracking();

        return await query.FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .AnyAsync(user => user.Email == normalizedEmail, cancellationToken);
    }

    public async Task<int> CountActiveAdminsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .CountAsync(user => user.Role == AppRoles.Admin && user.IsActive, cancellationToken);
    }

    public async Task<bool> HasFavoritesOrReviewsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var hasFavorites = await _context.FavoriteRecipes
            .AsNoTracking()
            .AnyAsync(favorite => favorite.UserId == userId, cancellationToken);

        if (hasFavorites)
        {
            return true;
        }

        var hasReviews = await _context.RecipeReviews
            .AsNoTracking()
            .AnyAsync(review => review.UserId == userId, cancellationToken);

        if (hasReviews)
        {
            return true;
        }

        var hasSocialData = await _context.UserFollows
            .AsNoTracking()
            .AnyAsync(follow => follow.FollowerUserId == userId || follow.FollowedUserId == userId, cancellationToken);

        if (hasSocialData)
        {
            return true;
        }

        hasSocialData = await _context.RecipeLikes
            .AsNoTracking()
            .AnyAsync(like => like.UserId == userId, cancellationToken);

        if (hasSocialData)
        {
            return true;
        }

        hasSocialData = await _context.RecipeComments
            .AsNoTracking()
            .AnyAsync(comment => comment.UserId == userId, cancellationToken);

        if (hasSocialData)
        {
            return true;
        }

        return await _context.Notifications
            .AsNoTracking()
            .AnyAsync(notification => notification.ActorUserId == userId || notification.RecipientUserId == userId, cancellationToken);
    }

    public async Task AddAsync(Users user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public void Update(Users user)
    {
        _context.Users.Update(user);
    }

    public void Delete(Users user)
    {
        _context.Users.Remove(user);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
