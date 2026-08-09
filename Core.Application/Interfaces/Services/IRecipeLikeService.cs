using Core.Application.Common;
using Core.Application.DTO.Social;
using Core.Application.DTO.Users;

namespace Core.Application.Interfaces.Services;

public interface IRecipeLikeService
{
    Task<ServiceResult> LikeAsync(Guid currentUserId, Guid recipeId, CancellationToken cancellationToken = default);
    Task<ServiceResult> UnlikeAsync(Guid currentUserId, Guid recipeId, CancellationToken cancellationToken = default);
    Task<ServiceResult<LikeStatusDto>> GetStatusAsync(Guid currentUserId, Guid recipeId, CancellationToken cancellationToken = default);
    Task<ServiceResult<PagedResult<UserSummaryDto>>> GetLikesAsync(Guid recipeId, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default);
}
