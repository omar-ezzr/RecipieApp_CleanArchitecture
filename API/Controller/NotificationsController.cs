using API.Extensions;
using Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller;

[ApiController]
[Authorize]
[Route("api/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return this.UnauthorizedIdentityProblem();
        }

        return Ok(await _notificationService.GetForUserAsync(currentUserId, page, pageSize, cancellationToken));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount(CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return this.UnauthorizedIdentityProblem();
        }

        return Ok(await _notificationService.GetUnreadCountAsync(currentUserId, cancellationToken));
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return this.UnauthorizedIdentityProblem();
        }

        var result = await _notificationService.MarkReadAsync(currentUserId, id, cancellationToken);
        return result.IsSuccess ? Ok() : this.ToActionResult(result);
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return this.UnauthorizedIdentityProblem();
        }

        var result = await _notificationService.MarkAllReadAsync(currentUserId, cancellationToken);
        return result.IsSuccess ? Ok() : this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return this.UnauthorizedIdentityProblem();
        }

        var result = await _notificationService.DeleteAsync(currentUserId, id, cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToActionResult(result);
    }

}
