using Core.Application.Common;
using Core.Application.DTO.Favorites;

namespace Core.Application.Interfaces.Services
{
    public interface IFavoriteService
    {
        Task<Result> AddFavoriteAsync(Guid userId, Guid recipeId);
        Task<Result> RemoveFavoriteAsync(Guid userId, Guid recipeId);
        Task<List<FavoriteRecipeDto>> GetUserFavoritesAsync(Guid userId);
        Task<bool> IsFavoriteAsync(Guid userId, Guid recipeId);
    }
}