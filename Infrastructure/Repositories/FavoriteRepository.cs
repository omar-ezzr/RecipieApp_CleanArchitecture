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

        public async Task<bool> ExistsAsync(Guid userId, Guid recipeId)
        {
            return await _context.FavoriteRecipes
                .AnyAsync(f => f.UserId == userId && f.RecipeId == recipeId);
        }

        public async Task AddAsync(FavoriteRecipe favorite)
        {
            await _context.FavoriteRecipes.AddAsync(favorite);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(Guid userId, Guid recipeId)
        {
            var favorite = await _context.FavoriteRecipes
                .FirstOrDefaultAsync(f => f.UserId == userId && f.RecipeId == recipeId);

            if (favorite == null)
            {
                return;
            }

            _context.FavoriteRecipes.Remove(favorite);
            await _context.SaveChangesAsync();
        }

        public async Task<List<FavoriteRecipe>> GetUserFavoritesAsync(Guid userId)
        {
            return await _context.FavoriteRecipes
                .Include(f => f.Recipe)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> RecipeExistsAsync(Guid recipeId)
        {
            return await _context.Recipies
                .AnyAsync(r => r.Id == recipeId);
        }
    }
}