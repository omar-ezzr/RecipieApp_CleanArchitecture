using Core.Application.Common;
using Core.Application.DTO.Social;

namespace Core.Application.Interfaces.Services;

public interface INotificationService
{
    Task<PagedResult<NotificationDto>> GetForUserAsync(Guid currentUserId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<UnreadNotificationCountDto> GetUnreadCountAsync(Guid currentUserId, CancellationToken cancellationToken = default);
    Task<ServiceResult> MarkReadAsync(Guid currentUserId, Guid notificationId, CancellationToken cancellationToken = default);
    Task<ServiceResult> MarkAllReadAsync(Guid currentUserId, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAsync(Guid currentUserId, Guid notificationId, CancellationToken cancellationToken = default);
}
