using Core.Application.Common;
using Core.Application.DTO.Social;
using Core.Domain.Entities;

namespace Core.Application.Interfaces.Repositories;

public interface IRecipeCommentRepository
{
    Task<(bool Exists, Guid? OwnerId)> GetRecipeStatusAsync(Guid recipeId, CancellationToken cancellationToken = default);
    Task<RecipeComment?> GetByIdAsync(Guid commentId, bool track = false, CancellationToken cancellationToken = default);
    Task AddAsync(RecipeComment comment, CancellationToken cancellationToken = default);
    Task DeleteAsync(RecipeComment comment);
    Task<PagedResult<RecipeCommentDto>> GetByRecipeAsync(Guid recipeId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<RecipeCommentDto?> GetDtoByIdAsync(Guid commentId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
