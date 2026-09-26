using API.Extensions;
using Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller;

[ApiController]
[Authorize]
[Route("api/feed")]
public sealed class FeedController : ControllerBase
{
    private readonly IFeedService _feedService;

    public FeedController(IFeedService feedService)
    {
        _feedService = feedService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return this.UnauthorizedIdentityProblem();
        }

        return Ok(await _feedService.GetFollowingFeedAsync(currentUserId, page, pageSize, cancellationToken));
    }
}
