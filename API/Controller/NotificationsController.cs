using API.Extensions;
using API.Responses;
using Core.Application.Common;
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
            return UnauthorizedIdentityProblem();
        }

        return Ok(await _notificationService.GetForUserAsync(currentUserId, page, pageSize, cancellationToken));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount(CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return UnauthorizedIdentityProblem();
        }

        return Ok(await _notificationService.GetUnreadCountAsync(currentUserId, cancellationToken));
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return UnauthorizedIdentityProblem();
        }

        var result = await _notificationService.MarkReadAsync(currentUserId, id, cancellationToken);
        return result.IsSuccess ? Ok() : ToActionResult(result);
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return UnauthorizedIdentityProblem();
        }

        var result = await _notificationService.MarkAllReadAsync(currentUserId, cancellationToken);
        return result.IsSuccess ? Ok() : ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return UnauthorizedIdentityProblem();
        }

        var result = await _notificationService.DeleteAsync(currentUserId, id, cancellationToken);
        return result.IsSuccess ? NoContent() : ToActionResult(result);
    }

    private IActionResult ToActionResult(ServiceResult result) => result.ErrorType switch
    {
        ServiceErrorType.NotFound => NotFound(Error("not_found", result.Error ?? "Resource was not found.")),
        ServiceErrorType.Forbidden => Forbid(),
        ServiceErrorType.Conflict => Conflict(Error("conflict", result.Error ?? "Conflict.")),
        ServiceErrorType.Validation => BadRequest(Error("validation_failed", result.Error ?? "The request is invalid.")),
        _ => BadRequest(Error("bad_request", result.Error ?? "The request is invalid."))
    };

    private UnauthorizedObjectResult UnauthorizedIdentityProblem() => Unauthorized(Error("invalid_identity", "Missing or malformed user identity claim"));
    private ApiErrorResponse Error(string code, string message) => new() { Code = code, Message = message, TraceId = HttpContext.TraceIdentifier };
}
