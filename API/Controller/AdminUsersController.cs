using Core.Application.Common;
using Core.Application.DTO.Users;
using Core.Application.Interfaces.Services;
using Core.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controller;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IUserManagementService _service;

    public AdminUsersController(IUserManagementService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] UserQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(parameters, cancellationToken);

        return ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);

        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserAccountDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateUserRoleDto dto, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();

        if (currentUserId is null)
        {
            return Unauthorized(new { message = "Missing or malformed user identity claim." });
        }

        var result = await _service.UpdateRoleAsync(currentUserId.Value, id, dto, cancellationToken);

        return ToActionResult(result);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateUserStatusDto dto, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();

        if (currentUserId is null)
        {
            return Unauthorized(new { message = "Missing or malformed user identity claim." });
        }

        var result = await _service.UpdateStatusAsync(currentUserId.Value, id, dto, cancellationToken);

        return ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();

        if (currentUserId is null)
        {
            return Unauthorized(new { message = "Missing or malformed user identity claim." });
        }

        var result = await _service.DeleteAsync(currentUserId.Value, id, cancellationToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return ToActionResult(result);
    }

    private Guid? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userId, out var parsed) ? parsed : null;
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return Error(result.Error!, result.ErrorType);
    }

    private IActionResult ToActionResult(ServiceResult result)
    {
        return Error(result.Error!, result.ErrorType);
    }

    private IActionResult Error(string message, ServiceErrorType type)
    {
        var body = new { message };

        return type switch
        {
            ServiceErrorType.NotFound => NotFound(body),
            ServiceErrorType.Conflict => Conflict(body),
            _ => BadRequest(body)
        };
    }
}
