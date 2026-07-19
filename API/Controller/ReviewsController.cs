using Core.Application.DTO.Reviews;
using Core.Application.Interfaces.Services;
using Core.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReviewDto dto, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return UnauthorizedIdentityProblem();
            }

            var result = await _reviewService.AddReviewAsync(userId.Value, dto, cancellationToken);

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

        [AllowAnonymous]
        [HttpGet("recipe/{recipeId}")]
        public async Task<IActionResult> GetByRecipe(Guid recipeId, CancellationToken cancellationToken)
        {
            var reviews = await _reviewService.GetRecipeReviewsAsync(recipeId, cancellationToken);

            return Ok(reviews);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReviewDto dto, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return UnauthorizedIdentityProblem();
            }

            var result = await _reviewService.UpdateReviewAsync(userId.Value, id, dto, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return UnauthorizedIdentityProblem();
            }

            var role = User.FindFirstValue(ClaimTypes.Role) ?? AppRoles.User;

            var result = await _reviewService.DeleteReviewAsync(userId.Value, role, id, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok();
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
