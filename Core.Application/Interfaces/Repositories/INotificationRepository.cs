using Core.Application.Common;
using Core.Application.DTO.Social;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Application.Interfaces.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task CreateAsync(Guid recipientUserId, Guid actorUserId, NotificationType type, Guid? recipeId, Guid? commentId, CancellationToken cancellationToken = default);
    Task<PagedResult<NotificationDto>> GetForUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Notification?> GetOwnedAsync(Guid notificationId, Guid userId, bool track = false, CancellationToken cancellationToken = default);
    Task MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Notification notification);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
