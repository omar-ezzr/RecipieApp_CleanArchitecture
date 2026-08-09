using Core.Application.Common;
using Core.Application.DTO;
using Core.Application.DTO.Recipe;

namespace Core.Application.Interfaces.Services;

public interface IRecipeService
{
Task<RecipieDto?> GetByIdAsync(Guid id, Guid? currentUserId = null, CancellationToken cancellationToken = default);
Task<ServiceResult<RecipieDto>> CreateAsync(CreateRecipeDto dto, Guid currentUserId, CancellationToken cancellationToken = default);
Task<ServiceResult<RecipieDto>> UpdateAsync(Guid id, CreateRecipeDto dto, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
Task<ServiceResult> DeleteAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
Task<PagedResult<RecipieDto>> GetPagedAsync(RecipeQueryParams parameters, Guid? currentUserId = null, CancellationToken cancellationToken = default);
Task<PagedResult<RecipieDto>> GetMineAsync(RecipeQueryParams parameters, Guid currentUserId, CancellationToken cancellationToken = default);
}
