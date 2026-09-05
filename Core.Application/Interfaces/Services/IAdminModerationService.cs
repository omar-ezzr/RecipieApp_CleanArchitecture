using Core.Application.Common;
using Core.Application.DTO.Admin;
namespace Core.Application.Interfaces.Services;
public interface IAdminModerationService
{
    Task<PagedResult<AdminRecipeListItemDto>> GetRecipesAsync(AdminListQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<AdminCommentListItemDto>> GetCommentsAsync(AdminListQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<AdminReviewListItemDto>> GetReviewsAsync(AdminListQuery query, CancellationToken cancellationToken = default);
}
