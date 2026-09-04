using Core.Application.DTO.Recipe;

namespace Core.Application.Interfaces.Services;

public interface IRecipeImageStorage
{
    Task<string> SaveAsync(RecipeImageUpload upload, CancellationToken cancellationToken = default);
    Task DeleteAsync(string imageUrl, CancellationToken cancellationToken = default);
}
