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

        public async Task<bool> RecipeExistsAsync(Guid recipeId, CancellationToken cancellationToken = default)
        {
            return await _context.Recipies
                .AsNoTracking()
                .AnyAsync(r => r.Id == recipeId, cancellationToken);
        }

        public async Task<bool> ExistsAsync(
            Guid userId,
            Guid recipeId,
            CancellationToken cancellationToken = default)
        {
            return await _context.RecipeReviews
                .AsNoTracking()
                .AnyAsync(r => r.UserId == userId && r.RecipeId == recipeId, cancellationToken);
        }

        public async Task AddAsync(RecipeReview review, CancellationToken cancellationToken = default)
        {
            await _context.RecipeReviews.AddAsync(review, cancellationToken);
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException("Duplicate review.", innerException: null);
            }
        }

        public async Task<RecipeReview?> GetByIdAsync(Guid reviewId, CancellationToken cancellationToken = default)
        {
            return await _context.RecipeReviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);
        }

        public async Task<List<RecipeReview>> GetByRecipeIdAsync(
            Guid recipeId,
            CancellationToken cancellationToken = default)
        {
            return await _context.RecipeReviews
                .Include(r => r.User)
                .AsNoTracking()
                .Where(r => r.RecipeId == recipeId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateAsync(RecipeReview review, CancellationToken cancellationToken = default)
        {
            _context.RecipeReviews.Update(review);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(RecipeReview review, CancellationToken cancellationToken = default)
        {
            _context.RecipeReviews.Remove(review);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
