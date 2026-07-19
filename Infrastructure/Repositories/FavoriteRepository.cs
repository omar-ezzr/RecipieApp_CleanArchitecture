using Core.Application.Interfaces.Repositories;
using Core.Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly AppDbContext _context;

        public FavoriteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(
            Guid userId,
            Guid recipeId,
            CancellationToken cancellationToken = default)
        {
            return await _context.FavoriteRecipes
                .AsNoTracking()
                .AnyAsync(f => f.UserId == userId && f.RecipeId == recipeId, cancellationToken);
        }

        public async Task AddAsync(FavoriteRecipe favorite, CancellationToken cancellationToken = default)
        {
            await _context.FavoriteRecipes.AddAsync(favorite, cancellationToken);
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException("Duplicate favorite.", innerException: null);
            }
        }

        public async Task RemoveAsync(Guid userId, Guid recipeId, CancellationToken cancellationToken = default)
        {
            var favorite = await _context.FavoriteRecipes
                .FirstOrDefaultAsync(f => f.UserId == userId && f.RecipeId == recipeId, cancellationToken);

            if (favorite == null)
            {
                return;
            }

            _context.FavoriteRecipes.Remove(favorite);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<FavoriteRecipe>> GetUserFavoritesAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.FavoriteRecipes
                .Include(f => f.Recipe)
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> RecipeExistsAsync(Guid recipeId, CancellationToken cancellationToken = default)
        {
            return await _context.Recipies
                .AsNoTracking()
                .AnyAsync(r => r.Id == recipeId, cancellationToken);
        }
    }
}
