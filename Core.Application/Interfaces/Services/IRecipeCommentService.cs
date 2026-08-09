using Core.Application.Common;
using Core.Application.DTO.Social;

namespace Core.Application.Interfaces.Services;

public interface IRecipeCommentService
{
    Task<ServiceResult<PagedResult<RecipeCommentDto>>> GetByRecipeAsync(Guid recipeId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ServiceResult<RecipeCommentDto>> CreateAsync(Guid currentUserId, Guid recipeId, CreateRecipeCommentDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<RecipeCommentDto>> UpdateAsync(Guid currentUserId, Guid commentId, UpdateRecipeCommentDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAsync(Guid currentUserId, bool isAdmin, Guid commentId, CancellationToken cancellationToken = default);
}
