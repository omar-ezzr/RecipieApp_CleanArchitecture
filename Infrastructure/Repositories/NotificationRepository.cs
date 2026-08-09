using Core.Application.Common;
using Core.Application.DTO.Social;
using Core.Application.DTO.Users;
using Core.Application.Interfaces.Repositories;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _context.Notifications.AddAsync(notification, cancellationToken);
    }

    public async Task CreateAsync(
        Guid recipientUserId,
        Guid actorUserId,
        NotificationType type,
        Guid? recipeId,
        Guid? commentId,
        CancellationToken cancellationToken = default)
    {
        if (recipientUserId == actorUserId)
        {
            return;
        }

        await AddAsync(new Notification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = recipientUserId,
            ActorUserId = actorUserId,
            Type = type,
            RecipeId = recipeId,
            CommentId = commentId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
    }

    public async Task<PagedResult<NotificationDto>> GetForUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 50);

        var query = _context.Notifications
            .AsNoTracking()
            .Where(notification => notification.RecipientUserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .ThenByDescending(notification => notification.Id);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .GroupJoin(
                _context.Recipies.AsNoTracking(),
                notification => notification.RecipeId,
                recipe => recipe.Id,
                (notification, recipes) => new { notification, recipes })
            .SelectMany(
                item => item.recipes.DefaultIfEmpty(),
                (item, recipe) => new NotificationDto
            {
                Id = item.notification.Id,
                Type = item.notification.Type.ToString(),
                Actor = new AuthorDto
                {
                    Id = item.notification.ActorUserId,
                    DisplayName = item.notification.ActorUser.DisplayName,
                    AvatarUrl = item.notification.ActorUser.AvatarUrl
                },
                RecipeId = item.notification.RecipeId,
                RecipeTitle = recipe == null ? null : recipe.Title,
                CommentId = item.notification.CommentId,
                IsRead = item.notification.IsRead,
                CreatedAt = item.notification.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<NotificationDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .AsNoTracking()
            .CountAsync(notification => notification.RecipientUserId == userId && !notification.IsRead, cancellationToken);
    }

    public async Task<Notification?> GetOwnedAsync(Guid notificationId, Guid userId, bool track = false, CancellationToken cancellationToken = default)
    {
        var query = track ? _context.Notifications.AsQueryable() : _context.Notifications.AsNoTracking();
        return await query.FirstOrDefaultAsync(
            notification => notification.Id == notificationId && notification.RecipientUserId == userId,
            cancellationToken);
    }

    public async Task MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _context.Notifications
            .Where(notification => notification.RecipientUserId == userId && !notification.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }
    }

    public Task DeleteAsync(Notification notification)
    {
        _context.Notifications.Remove(notification);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
