using Core.Application.Interfaces.Services;

using Core.Application.Common;
using Core.Application.DTO.Favorites;
using Core.Domain.Entities;
using Core.Application.Interfaces.Repositories;

namespace Core.Application.UseCases.Favorites
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepository;

        public FavoriteService(IFavoriteRepository favoriteRepository)
        {
            _favoriteRepository = favoriteRepository;
        }

        public async Task<Result> AddFavoriteAsync(
            Guid userId,
            Guid recipeId,
            CancellationToken cancellationToken = default)
        {
            var recipeExists = await _favoriteRepository.RecipeExistsAsync(recipeId, cancellationToken);

            if (!recipeExists)
            {
                return Result.Failure("Recipe not found.");
            }

            var alreadyExists = await _favoriteRepository.ExistsAsync(userId, recipeId, cancellationToken);

            if (alreadyExists)
            {
                return Result.Failure("Recipe is already in favorites.");
            }

            var favorite = new FavoriteRecipe
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RecipeId = recipeId,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _favoriteRepository.AddAsync(favorite, cancellationToken);
            }
            catch (InvalidOperationException)
            {
                return Result.Failure("Recipe is already in favorites.");
            }

            return Result.Success();
        }

        public async Task<Result> RemoveFavoriteAsync(
            Guid userId,
            Guid recipeId,
            CancellationToken cancellationToken = default)
        {
            var alreadyExists = await _favoriteRepository.ExistsAsync(userId, recipeId, cancellationToken);

            if (!alreadyExists)
            {
                return Result.Failure("Favorite not found.");
            }

            await _favoriteRepository.RemoveAsync(userId, recipeId, cancellationToken);

            return Result.Success();
        }

        public async Task<List<FavoriteRecipeDto>> GetUserFavoritesAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var favorites = await _favoriteRepository.GetUserFavoritesAsync(userId, cancellationToken);

            return favorites.Select(f => new FavoriteRecipeDto
            {
                Id = f.Id,
                RecipeId = f.RecipeId,
                RecipeTitle = f.Recipe.Title,
                RecipeImageUrl = f.Recipe.ImageUrl,
                CreatedAt = f.CreatedAt
            }).ToList();
        }

        public async Task<bool> IsFavoriteAsync(
            Guid userId,
            Guid recipeId,
            CancellationToken cancellationToken = default)
        {
            return await _favoriteRepository.ExistsAsync(userId, recipeId, cancellationToken);
        }
    }
}
