using Core.Application.Common;
using Core.Application.DTO.Social;
using Core.Application.DTO.Users;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Application.UseCases.Social;

public sealed class UserFollowService : IUserFollowService
{
    private readonly IUserFollowRepository _followRepository;
    private readonly INotificationRepository _notificationRepository;

    public UserFollowService(IUserFollowRepository followRepository, INotificationRepository notificationRepository)
    {
        _followRepository = followRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task<ServiceResult> FollowAsync(Guid currentUserId, Guid targetUserId, CancellationToken cancellationToken = default)
    {
        if (currentUserId == targetUserId)
        {
            return ServiceResult.Failure("You cannot follow yourself.", ServiceErrorType.Validation);
        }

        if (!await _followRepository.ActiveUserExistsAsync(targetUserId, cancellationToken))
        {
            return ServiceResult.Failure("User was not found.", ServiceErrorType.NotFound);
        }

        if (await _followRepository.ExistsAsync(currentUserId, targetUserId, cancellationToken))
        {
            return ServiceResult.Failure("You already follow this user.", ServiceErrorType.Conflict);
        }

        await _followRepository.AddAsync(new UserFollow
        {
            Id = Guid.NewGuid(),
            FollowerUserId = currentUserId,
            FollowedUserId = targetUserId,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
        await _notificationRepository.CreateAsync(targetUserId, currentUserId, NotificationType.Follow, null, null, cancellationToken);
        await _followRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UnfollowAsync(Guid currentUserId, Guid targetUserId, CancellationToken cancellationToken = default)
    {
        if (currentUserId == targetUserId)
        {
            return ServiceResult.Failure("You cannot unfollow yourself.", ServiceErrorType.Validation);
        }

        if (!await _followRepository.ActiveUserExistsAsync(targetUserId, cancellationToken))
        {
            return ServiceResult.Failure("User was not found.", ServiceErrorType.NotFound);
        }

        await _followRepository.RemoveAsync(currentUserId, targetUserId, cancellationToken);
        await _followRepository.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult<FollowStatusDto>> GetStatusAsync(Guid currentUserId, Guid targetUserId, CancellationToken cancellationToken = default)
    {
        if (!await _followRepository.ActiveUserExistsAsync(targetUserId, cancellationToken))
        {
            return ServiceResult<FollowStatusDto>.Failure("User was not found.", ServiceErrorType.NotFound);
        }

        var isFollowing = await _followRepository.ExistsAsync(currentUserId, targetUserId, cancellationToken);
        return ServiceResult<FollowStatusDto>.Success(new FollowStatusDto { IsFollowing = isFollowing });
    }

    public async Task<ServiceResult<PagedResult<UserSummaryDto>>> GetFollowersAsync(Guid userId, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (!await _followRepository.ActiveUserExistsAsync(userId, cancellationToken))
        {
            return ServiceResult<PagedResult<UserSummaryDto>>.Failure("User was not found.", ServiceErrorType.NotFound);
        }

        return ServiceResult<PagedResult<UserSummaryDto>>.Success(await _followRepository.GetFollowersAsync(userId, currentUserId, page, pageSize, cancellationToken));
    }

    public async Task<ServiceResult<PagedResult<UserSummaryDto>>> GetFollowingAsync(Guid userId, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (!await _followRepository.ActiveUserExistsAsync(userId, cancellationToken))
        {
            return ServiceResult<PagedResult<UserSummaryDto>>.Failure("User was not found.", ServiceErrorType.NotFound);
        }

        return ServiceResult<PagedResult<UserSummaryDto>>.Success(await _followRepository.GetFollowingAsync(userId, currentUserId, page, pageSize, cancellationToken));
    }
}
