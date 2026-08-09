using API.Extensions;
using API.Responses;
using Core.Application.Common;
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
            return UnauthorizedIdentityProblem();
        }

        var result = await _likeService.LikeAsync(currentUserId, recipeId, cancellationToken);
        return result.IsSuccess ? Ok() : ToActionResult(result);
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> Unlike(Guid recipeId, CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return UnauthorizedIdentityProblem();
        }

        var result = await _likeService.UnlikeAsync(currentUserId, recipeId, cancellationToken);
        return result.IsSuccess ? NoContent() : ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetLikes(Guid recipeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var currentUserId = User.TryGetCurrentUserId(out var parsed) ? parsed : (Guid?)null;
        var result = await _likeService.GetLikesAsync(recipeId, currentUserId, page, pageSize, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToActionResult(result);
    }

    [Authorize]
    [HttpGet("status")]
    public async Task<IActionResult> Status(Guid recipeId, CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return UnauthorizedIdentityProblem();
        }

        var result = await _likeService.GetStatusAsync(currentUserId, recipeId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToActionResult(result);
    }

    private IActionResult ToActionResult(ServiceResult result) => result.ErrorType switch
    {
        ServiceErrorType.NotFound => NotFound(Error("not_found", result.Error ?? "Resource was not found.")),
        ServiceErrorType.Conflict => Conflict(Error("conflict", result.Error ?? "Conflict.")),
        ServiceErrorType.Validation => BadRequest(Error("validation_failed", result.Error ?? "The request is invalid.")),
        ServiceErrorType.Forbidden => Forbid(),
        _ => BadRequest(Error("bad_request", result.Error ?? "The request is invalid."))
    };

    private IActionResult ToActionResult<T>(ServiceResult<T> result) => result.ErrorType switch
    {
        ServiceErrorType.NotFound => NotFound(Error("not_found", result.Error ?? "Resource was not found.")),
        ServiceErrorType.Conflict => Conflict(Error("conflict", result.Error ?? "Conflict.")),
        ServiceErrorType.Validation => BadRequest(Error("validation_failed", result.Error ?? "The request is invalid.")),
        ServiceErrorType.Forbidden => Forbid(),
        _ => BadRequest(Error("bad_request", result.Error ?? "The request is invalid."))
    };

    private UnauthorizedObjectResult UnauthorizedIdentityProblem() => Unauthorized(Error("invalid_identity", "Missing or malformed user identity claim"));
    private ApiErrorResponse Error(string code, string message) => new() { Code = code, Message = message, TraceId = HttpContext.TraceIdentifier };
}
