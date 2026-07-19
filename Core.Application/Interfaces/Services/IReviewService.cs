using Core.Application.Common;
using Core.Application.DTO.Reviews;

namespace Core.Application.Interfaces.Services
{
    public interface IReviewService
    {
        Task<Result> AddReviewAsync(Guid userId, CreateReviewDto dto, CancellationToken cancellationToken = default);
        Task<Result> UpdateReviewAsync(Guid userId, Guid reviewId, UpdateReviewDto dto, CancellationToken cancellationToken = default);
        Task<Result> DeleteReviewAsync(Guid userId, string role, Guid reviewId, CancellationToken cancellationToken = default);
        Task<List<ReviewDto>> GetRecipeReviewsAsync(Guid recipeId, CancellationToken cancellationToken = default);
    }
}
