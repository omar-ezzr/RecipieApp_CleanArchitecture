using Core.Application.Common;
using Core.Application.DTO;
using Core.Application.DTO.Recipe;
using Core.Application.DTO.Users;

namespace Core.Application.Interfaces.Services;

public interface IUserProfileService
{
    Task<ServiceResult<PublicUserProfileDto>> GetPublicProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ServiceResult<PagedResult<RecipieDto>>> GetUserRecipesAsync(Guid userId, RecipeQueryParams parameters, CancellationToken cancellationToken = default);
    Task<ServiceResult<PublicUserProfileDto>> UpdateCurrentProfileAsync(Guid currentUserId, UpdatePublicUserProfileDto dto, CancellationToken cancellationToken = default);
}
