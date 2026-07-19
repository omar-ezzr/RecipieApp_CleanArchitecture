using Core.Application.Common;
using Core.Application.DTO.Favorites;

namespace Core.Application.Interfaces.Services
{
    public interface IFavoriteService
    {
        Task<Result> AddFavoriteAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default);
        Task<Result> RemoveFavoriteAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default);
        Task<List<FavoriteRecipeDto>> GetUserFavoritesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> IsFavoriteAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default);
    }
}
