using Core.Application.Interfaces.Services;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;
        private readonly AppDbContext _context;

        public FavoritesController(
            IFavoriteService favoriteService,
            AppDbContext context)
        {
            _favoriteService = favoriteService;
            _context = context;
        }

        [HttpPost("{recipeId}")]
        public async Task<IActionResult> Add(Guid recipeId)
        {
            var userId = await GetCurrentUserIdAsync();

            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _favoriteService.AddFavoriteAsync(userId.Value, recipeId);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [HttpDelete("{recipeId}")]
        public async Task<IActionResult> Remove(Guid recipeId)
        {
            var userId = await GetCurrentUserIdAsync();

            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _favoriteService.RemoveFavoriteAsync(userId.Value, recipeId);

            if (!result.IsSuccess)
            {
                return NotFound(result.Error);
            }

            return Ok();
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMine()
        {
            var userId = await GetCurrentUserIdAsync();

            if (userId == null)
            {
                return Unauthorized();
            }

            var favorites = await _favoriteService.GetUserFavoritesAsync(userId.Value);

            return Ok(favorites);
        }

        [HttpGet("check/{recipeId}")]
        public async Task<IActionResult> Check(Guid recipeId)
        {
            var userId = await GetCurrentUserIdAsync();

            if (userId == null)
            {
                return Unauthorized();
            }

            var isFavorite = await _favoriteService.IsFavoriteAsync(userId.Value, recipeId);

            return Ok(new
            {
                isFavorite
            });
        }

        private async Task<Guid?> GetCurrentUserIdAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            return user?.Id;
        }
    }
}