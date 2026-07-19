using Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoritesController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        [HttpPost("{recipeId}")]
        public async Task<IActionResult> Add(Guid recipeId, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return UnauthorizedIdentityProblem();
            }

            var result = await _favoriteService.AddFavoriteAsync(userId.Value, recipeId, cancellationToken);

            if (!result.IsSuccess)
            {
                return Conflict(new ProblemDetails
                {
                    Title = result.Error,
                    Status = StatusCodes.Status409Conflict
                });
            }

            return Ok();
        }

        [HttpDelete("{recipeId}")]
        public async Task<IActionResult> Remove(Guid recipeId, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return UnauthorizedIdentityProblem();
            }

            var result = await _favoriteService.RemoveFavoriteAsync(userId.Value, recipeId, cancellationToken);

            if (!result.IsSuccess)
            {
                return NotFound(result.Error);
            }

            return Ok();
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return UnauthorizedIdentityProblem();
            }

            var favorites = await _favoriteService.GetUserFavoritesAsync(userId.Value, cancellationToken);

            return Ok(favorites);
        }

        [HttpGet("check/{recipeId}")]
        public async Task<IActionResult> Check(Guid recipeId, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return UnauthorizedIdentityProblem();
            }

            var isFavorite = await _favoriteService.IsFavoriteAsync(userId.Value, recipeId, cancellationToken);

            return Ok(new
            {
                isFavorite
            });
        }

        private Guid? GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userId, out var parsed) ? parsed : null;
        }

        private UnauthorizedObjectResult UnauthorizedIdentityProblem() =>
            Unauthorized(new ProblemDetails
            {
                Title = "Missing or malformed user identity claim",
                Status = StatusCodes.Status401Unauthorized
            });
    }
}
