using Core.Application.Common;
using Core.Application.DTO.Social;

namespace Core.Application.Interfaces.Repositories;

public interface IFeedRepository
{
    Task<PagedResult<FeedRecipeDto>> GetFollowingFeedAsync(Guid currentUserId, int page, int pageSize, CancellationToken cancellationToken = default);
}
