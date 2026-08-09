using Core.Application.Common;
using Core.Application.DTO.Users;
using Core.Domain.Entities;

namespace Core.Application.Interfaces.Repositories;

public interface IRecipeLikeRepository
{
    Task<(bool Exists, Guid? OwnerId)> GetRecipeStatusAsync(Guid recipeId, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid recipeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default);
    Task AddAsync(RecipeLike like, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default);
    Task<PagedResult<UserSummaryDto>> GetRecipeLikesAsync(Guid recipeId, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
