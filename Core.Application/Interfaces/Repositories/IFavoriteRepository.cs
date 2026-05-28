using Core.Domain.Entities;

namespace Core.Application.Interfaces.Repositories
{
    public interface IFavoriteRepository
    {
        Task<bool> ExistsAsync(Guid userId, Guid recipeId);
        Task AddAsync(FavoriteRecipe favorite);
        Task RemoveAsync(Guid userId, Guid recipeId);
        Task<List<FavoriteRecipe>> GetUserFavoritesAsync(Guid userId);
        Task<bool> RecipeExistsAsync(Guid recipeId);
    }
}