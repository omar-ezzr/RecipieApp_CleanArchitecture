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

        public async Task<Result> AddFavoriteAsync(Guid userId, Guid recipeId)
        {
            var recipeExists = await _favoriteRepository.RecipeExistsAsync(recipeId);

            if (!recipeExists)
            {
                return Result.Failure("Recipe not found.");
            }

            var alreadyExists = await _favoriteRepository.ExistsAsync(userId, recipeId);

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

            await _favoriteRepository.AddAsync(favorite);

            return Result.Success();
        }

        public async Task<Result> RemoveFavoriteAsync(Guid userId, Guid recipeId)
        {
            var alreadyExists = await _favoriteRepository.ExistsAsync(userId, recipeId);

            if (!alreadyExists)
            {
                return Result.Failure("Favorite not found.");
            }

            await _favoriteRepository.RemoveAsync(userId, recipeId);

            return Result.Success();
        }

        public async Task<List<FavoriteRecipeDto>> GetUserFavoritesAsync(Guid userId)
        {
            var favorites = await _favoriteRepository.GetUserFavoritesAsync(userId);

            return favorites.Select(f => new FavoriteRecipeDto
            {
                Id = f.Id,
                RecipeId = f.RecipeId,
                RecipeTitle = f.Recipe.Title,
                RecipeImageUrl = f.Recipe.ImageUrl,
                CreatedAt = f.CreatedAt
            }).ToList();
        }

        public async Task<bool> IsFavoriteAsync(Guid userId, Guid recipeId)
        {
            return await _favoriteRepository.ExistsAsync(userId, recipeId);
        }
    }
}