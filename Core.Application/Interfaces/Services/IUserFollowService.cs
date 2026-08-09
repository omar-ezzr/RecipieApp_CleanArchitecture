using Core.Application.Common;
using Core.Application.DTO.Social;
using Core.Application.DTO.Users;

namespace Core.Application.Interfaces.Services;

public interface IUserFollowService
{
    Task<ServiceResult> FollowAsync(Guid currentUserId, Guid targetUserId, CancellationToken cancellationToken = default);
    Task<ServiceResult> UnfollowAsync(Guid currentUserId, Guid targetUserId, CancellationToken cancellationToken = default);
    Task<ServiceResult<FollowStatusDto>> GetStatusAsync(Guid currentUserId, Guid targetUserId, CancellationToken cancellationToken = default);
    Task<ServiceResult<PagedResult<UserSummaryDto>>> GetFollowersAsync(Guid userId, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ServiceResult<PagedResult<UserSummaryDto>>> GetFollowingAsync(Guid userId, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default);
}
