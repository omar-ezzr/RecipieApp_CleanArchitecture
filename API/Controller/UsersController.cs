using API.Extensions;
using Core.Application.DTO;
using Core.Application.DTO.Users;
using Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserProfileService _profileService;
    private readonly IUserFollowService _followService;

    public UsersController(IUserProfileService profileService, IUserFollowService followService)
    {
        _profileService = profileService;
        _followService = followService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _profileService.GetPublicProfileAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToActionResult(result);
    }

    [HttpGet("{id:guid}/recipes")]
    public async Task<IActionResult> GetRecipes(Guid id, [FromQuery] RecipeQueryParams parameters, CancellationToken cancellationToken)
    {
        var result = await _profileService.GetUserRecipesAsync(id, parameters, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToActionResult(result);
    }

    [Authorize]
    [HttpGet("me/profile")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return this.UnauthorizedIdentityProblem();
        }

        var result = await _profileService.GetPublicProfileAsync(currentUserId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToActionResult(result);
    }

    [Authorize]
    [HttpPut("me/profile")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdatePublicUserProfileDto dto, CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return this.UnauthorizedIdentityProblem();
        }

        var result = await _profileService.UpdateCurrentProfileAsync(currentUserId, dto, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToActionResult(result);
    }

    [Authorize]
    [HttpPost("{userId:guid}/follow")]
    public async Task<IActionResult> Follow(Guid userId, CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return this.UnauthorizedIdentityProblem();
        }

        var result = await _followService.FollowAsync(currentUserId, userId, cancellationToken);
        return result.IsSuccess ? Ok() : this.ToActionResult(result);
    }

    [Authorize]
    [HttpDelete("{userId:guid}/follow")]
    public async Task<IActionResult> Unfollow(Guid userId, CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return this.UnauthorizedIdentityProblem();
        }

        var result = await _followService.UnfollowAsync(currentUserId, userId, cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToActionResult(result);
    }

    [HttpGet("{userId:guid}/followers")]
    public async Task<IActionResult> Followers(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var currentUserId = User.TryGetCurrentUserId(out var parsed) ? parsed : (Guid?)null;
        var result = await _followService.GetFollowersAsync(userId, currentUserId, page, pageSize, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToActionResult(result);
    }

    [HttpGet("{userId:guid}/following")]
    public async Task<IActionResult> Following(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var currentUserId = User.TryGetCurrentUserId(out var parsed) ? parsed : (Guid?)null;
        var result = await _followService.GetFollowingAsync(userId, currentUserId, page, pageSize, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToActionResult(result);
    }

    [Authorize]
    [HttpGet("{userId:guid}/follow-status")]
    public async Task<IActionResult> FollowStatus(Guid userId, CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return this.UnauthorizedIdentityProblem();
        }

        var result = await _followService.GetStatusAsync(currentUserId, userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToActionResult(result);
    }

}
