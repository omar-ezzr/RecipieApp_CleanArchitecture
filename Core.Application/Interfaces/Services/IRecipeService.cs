using Core.Application.Common;
using Core.Application.DTO;
using Core.Application.DTO.Recipe;

namespace Core.Application.Interfaces.Services;

public interface IRecipeService
{
Task<IEnumerable<RecipieDto>> GetAllAsync();
Task<RecipieDto?> GetByIdAsync(Guid id);
Task<Result> CreateAsync(CreateRecipeDto dto);
Task<Result> UpdateAsync(Guid id, CreateRecipeDto dto);
Task<Result> DeleteAsync(Guid id);
Task<(List<RecipieDto>, int)> GetPagedAsync(RecipeQueryParams parameters);
}
