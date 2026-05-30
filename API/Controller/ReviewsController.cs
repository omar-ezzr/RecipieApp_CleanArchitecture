using Core.Application.DTO.Reviews;
using Core.Application.Interfaces.Services;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly AppDbContext _context;

        public ReviewsController(
            IReviewService reviewService,
            AppDbContext context)
        {
            _reviewService = reviewService;
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
        {
            var userId = await GetCurrentUserIdAsync();

            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _reviewService.AddReviewAsync(userId.Value, dto);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("recipe/{recipeId}")]
        public async Task<IActionResult> GetByRecipe(Guid recipeId)
        {
            var reviews = await _reviewService.GetRecipeReviewsAsync(recipeId);

            return Ok(reviews);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReviewDto dto)
        {
            var userId = await GetCurrentUserIdAsync();

            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _reviewService.UpdateReviewAsync(userId.Value, id, dto);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = await GetCurrentUserIdAsync();

            if (userId == null)
            {
                return Unauthorized();
            }

            var role = User.FindFirstValue(ClaimTypes.Role) ?? "User";

            var result = await _reviewService.DeleteReviewAsync(userId.Value, role, id);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Ok();
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