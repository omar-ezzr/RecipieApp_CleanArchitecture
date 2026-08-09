using Core.Application.Common;
using Core.Application.DTO.Reviews;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Domain.Constants;
using Core.Domain.Entities;
using Core.Application.DTO.Users;

namespace Core.Application.UseCases.Reviews
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;

        public ReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<Result> AddReviewAsync(
            Guid userId,
            CreateReviewDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
            {
                return Result.Failure("Rating must be between 1 and 5.");
            }

            var recipeExists = await _reviewRepository.RecipeExistsAsync(dto.RecipeId, cancellationToken);

            if (!recipeExists)
            {
                return Result.Failure("Recipe not found.");
            }

            var alreadyReviewed = await _reviewRepository.ExistsAsync(userId, dto.RecipeId, cancellationToken);

            if (alreadyReviewed)
            {
                return Result.Failure("You already reviewed this recipe.");
            }

            var review = new RecipeReview
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RecipeId = dto.RecipeId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _reviewRepository.AddAsync(review, cancellationToken);
            }
            catch (InvalidOperationException)
            {
                return Result.Failure("You already reviewed this recipe.");
            }

            return Result.Success();
        }

        public async Task<Result> UpdateReviewAsync(
            Guid userId,
            Guid reviewId,
            UpdateReviewDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
            {
                return Result.Failure("Rating must be between 1 and 5.");
            }

            var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);

            if (review == null)
            {
                return Result.Failure("Review not found.");
            }

            if (review.UserId != userId)
            {
                return Result.Failure("You can only update your own review.");
            }

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.UpdatedAt = DateTime.UtcNow;

            await _reviewRepository.UpdateAsync(review, cancellationToken);

            return Result.Success();
        }

        public async Task<Result> DeleteReviewAsync(
            Guid userId,
            string role,
            Guid reviewId,
            CancellationToken cancellationToken = default)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);

            if (review == null)
            {
                return Result.Failure("Review not found.");
            }

            var isAdmin = role == AppRoles.Admin;

            if (!isAdmin && review.UserId != userId)
            {
                return Result.Failure("You can only delete your own review.");
            }

            await _reviewRepository.DeleteAsync(review, cancellationToken);

            return Result.Success();
        }

        public async Task<List<ReviewDto>> GetRecipeReviewsAsync(
            Guid recipeId,
            CancellationToken cancellationToken = default)
        {
            var reviews = await _reviewRepository.GetByRecipeIdAsync(recipeId, cancellationToken);

            return reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                RecipeId = r.RecipeId,
                UserId = r.UserId,
                Author = new AuthorDto
                {
                    Id = r.UserId,
                    DisplayName = r.User.DisplayName,
                    AvatarUrl = r.User.AvatarUrl
                },
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToList();
        }
    }
}
