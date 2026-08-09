using Core.Application.Common;
using Core.Application.DTO.Users;
using Core.Domain.Entities;

namespace Core.Application.Interfaces.Repositories;

public interface IUserFollowRepository
{
    Task<bool> ActiveUserExistsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid followerUserId, Guid followedUserId, CancellationToken cancellationToken = default);
    Task AddAsync(UserFollow follow, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid followerUserId, Guid followedUserId, CancellationToken cancellationToken = default);
    Task<PagedResult<UserSummaryDto>> GetFollowersAsync(Guid userId, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<UserSummaryDto>> GetFollowingAsync(Guid userId, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
