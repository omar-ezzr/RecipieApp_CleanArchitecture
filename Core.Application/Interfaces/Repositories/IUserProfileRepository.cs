using Core.Application.Common;
using Core.Application.DTO;
using Core.Application.DTO.Recipe;
using Core.Application.DTO.Users;
using Core.Domain.Entities;

namespace Core.Application.Interfaces.Repositories;

public interface IUserProfileRepository
{
    Task<PublicUserProfileDto?> GetPublicProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Users?> GetActiveUserForUpdateAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PagedResult<RecipieDto>> GetUserRecipesAsync(Guid userId, RecipeQueryParams parameters, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
