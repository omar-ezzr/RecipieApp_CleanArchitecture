using Core.Domain.Entities;

namespace Core.Application.Interfaces.Repositories
{
    public interface IReviewRepository
    {
        Task<bool> RecipeExistsAsync(Guid recipeId, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default);
        Task AddAsync(RecipeReview review, CancellationToken cancellationToken = default);
        Task<RecipeReview?> GetByIdAsync(Guid reviewId, CancellationToken cancellationToken = default);
        Task<List<RecipeReview>> GetByRecipeIdAsync(Guid recipeId, CancellationToken cancellationToken = default);
        Task UpdateAsync(RecipeReview review, CancellationToken cancellationToken = default);
        Task DeleteAsync(RecipeReview review, CancellationToken cancellationToken = default);
    }
}
