using Core.Domain.Entities;

namespace Core.Application.Interfaces.Repositories
{
    public interface IReviewRepository
    {
        Task<bool> RecipeExistsAsync(Guid recipeId);
        Task<bool> ExistsAsync(Guid userId, Guid recipeId);
        Task AddAsync(RecipeReview review);
        Task<RecipeReview?> GetByIdAsync(Guid reviewId);
        Task<List<RecipeReview>> GetByRecipeIdAsync(Guid recipeId);
        Task UpdateAsync(RecipeReview review);
        Task DeleteAsync(RecipeReview review);
    }
}