using Core.Application.Common;
using Core.Application.DTO.Social;

namespace Core.Application.Interfaces.Services;

public interface IFeedService
{
    Task<PagedResult<FeedRecipeDto>> GetFollowingFeedAsync(Guid currentUserId, int page, int pageSize, CancellationToken cancellationToken = default);
}
