using Core.Domain.Entities;

namespace Core.Application.Interfaces.Repositories
{
    public interface IFavoriteRepository
    {
        Task<bool> ExistsAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default);
        Task AddAsync(FavoriteRecipe favorite, CancellationToken cancellationToken = default);
        Task RemoveAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default);
        Task<List<FavoriteRecipe>> GetUserFavoritesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> RecipeExistsAsync(Guid recipeId, CancellationToken cancellationToken = default);
    }
}
