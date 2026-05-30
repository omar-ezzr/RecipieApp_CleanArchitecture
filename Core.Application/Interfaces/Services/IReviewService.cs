using Core.Application.Common;
using Core.Application.DTO.Reviews;

namespace Core.Application.Interfaces.Services
{
    public interface IReviewService
    {
        Task<Result> AddReviewAsync(Guid userId, CreateReviewDto dto);
        Task<Result> UpdateReviewAsync(Guid userId, Guid reviewId, UpdateReviewDto dto);
        Task<Result> DeleteReviewAsync(Guid userId, string role, Guid reviewId);
        Task<List<ReviewDto>> GetRecipeReviewsAsync(Guid recipeId);
    }
}