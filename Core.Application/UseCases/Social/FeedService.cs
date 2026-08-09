using Core.Application.Common;
using Core.Application.DTO.Social;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;

namespace Core.Application.UseCases.Social;

public sealed class FeedService : IFeedService
{
    private readonly IFeedRepository _repository;

    public FeedService(IFeedRepository repository)
    {
        _repository = repository;
    }

    public Task<PagedResult<FeedRecipeDto>> GetFollowingFeedAsync(Guid currentUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return _repository.GetFollowingFeedAsync(currentUserId, page, pageSize, cancellationToken);
    }
}
