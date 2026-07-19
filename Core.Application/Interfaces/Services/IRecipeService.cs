using Core.Application.Common;
using Core.Application.DTO;
using Core.Application.DTO.Recipe;

namespace Core.Application.Interfaces.Services;

public interface IRecipeService
{
Task<RecipieDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
Task<Result> CreateAsync(CreateRecipeDto dto, CancellationToken cancellationToken = default);
Task<Result> UpdateAsync(Guid id, CreateRecipeDto dto, CancellationToken cancellationToken = default);
Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
Task<PagedResult<RecipieDto>> GetPagedAsync(RecipeQueryParams parameters, CancellationToken cancellationToken = default);
}
