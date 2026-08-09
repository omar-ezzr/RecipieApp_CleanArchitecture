using Core.Application.Common;
using Core.Application.DTO.Social;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;

namespace Core.Application.UseCases.Social;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;

    public NotificationService(INotificationRepository repository)
    {
        _repository = repository;
    }

    public Task<PagedResult<NotificationDto>> GetForUserAsync(Guid currentUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return _repository.GetForUserAsync(currentUserId, page, pageSize, cancellationToken);
    }

    public async Task<UnreadNotificationCountDto> GetUnreadCountAsync(Guid currentUserId, CancellationToken cancellationToken = default)
    {
        return new UnreadNotificationCountDto
        {
            Count = await _repository.GetUnreadCountAsync(currentUserId, cancellationToken)
        };
    }

    public async Task<ServiceResult> MarkReadAsync(Guid currentUserId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _repository.GetOwnedAsync(notificationId, currentUserId, track: true, cancellationToken);
        if (notification is null)
        {
            return ServiceResult.Failure("Notification was not found.", ServiceErrorType.NotFound);
        }

        notification.IsRead = true;
        await _repository.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> MarkAllReadAsync(Guid currentUserId, CancellationToken cancellationToken = default)
    {
        await _repository.MarkAllReadAsync(currentUserId, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(Guid currentUserId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _repository.GetOwnedAsync(notificationId, currentUserId, track: true, cancellationToken);
        if (notification is null)
        {
            return ServiceResult.Failure("Notification was not found.", ServiceErrorType.NotFound);
        }

        await _repository.DeleteAsync(notification);
        await _repository.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }
}
