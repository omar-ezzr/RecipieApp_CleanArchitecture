using API.Extensions;
using Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller;

[ApiController]
[Route("api/recipes/{recipeId:guid}/likes")]
public sealed class RecipeLikesController : ControllerBase
{
    private readonly IRecipeLikeService _likeService;

    public RecipeLikesController(IRecipeLikeService likeService)
    {
        _likeService = likeService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Like(Guid recipeId, CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return this.UnauthorizedIdentityProblem();
        }

        var result = await _likeService.LikeAsync(currentUserId, recipeId, cancellationToken);
        return result.IsSuccess ? Ok() : this.ToActionResult(result);
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> Unlike(Guid recipeId, CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return this.UnauthorizedIdentityProblem();
        }

        var result = await _likeService.UnlikeAsync(currentUserId, recipeId, cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetLikes(Guid recipeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var currentUserId = User.TryGetCurrentUserId(out var parsed) ? parsed : (Guid?)null;
        var result = await _likeService.GetLikesAsync(recipeId, currentUserId, page, pageSize, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToActionResult(result);
    }

    [Authorize]
    [HttpGet("status")]
    public async Task<IActionResult> Status(Guid recipeId, CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return this.UnauthorizedIdentityProblem();
        }

        var result = await _likeService.GetStatusAsync(currentUserId, recipeId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToActionResult(result);
    }

}
