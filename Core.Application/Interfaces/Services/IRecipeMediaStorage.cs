using Core.Application.DTO.Recipe;
namespace Core.Application.Interfaces.Services;
public interface IRecipeMediaStorage { Task<string> SaveAsync(RecipeMediaUpload upload, CancellationToken cancellationToken = default); Task DeleteAsync(string url, CancellationToken cancellationToken = default); }
