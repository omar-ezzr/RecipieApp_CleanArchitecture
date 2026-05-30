using Core.Application.Interfaces.Repositories;
using Core.Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RecipeExistsAsync(Guid recipeId)
        {
            return await _context.Recipies
                .AnyAsync(r => r.Id == recipeId);
        }

        public async Task<bool> ExistsAsync(Guid userId, Guid recipeId)
        {
            return await _context.RecipeReviews
                .AnyAsync(r => r.UserId == userId && r.RecipeId == recipeId);
        }

        public async Task AddAsync(RecipeReview review)
        {
            await _context.RecipeReviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task<RecipeReview?> GetByIdAsync(Guid reviewId)
        {
            return await _context.RecipeReviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == reviewId);
        }

        public async Task<List<RecipeReview>> GetByRecipeIdAsync(Guid recipeId)
        {
            return await _context.RecipeReviews
                .Include(r => r.User)
                .Where(r => r.RecipeId == recipeId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(RecipeReview review)
        {
            _context.RecipeReviews.Update(review);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(RecipeReview review)
        {
            _context.RecipeReviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }
}