using Core.Application.DTO.Admin;
using Core.Application.Interfaces.Services;
using Core.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace API.Controller;
[ApiController, Route("api/admin"), Authorize(Roles = AppRoles.Admin)]
public sealed class AdminModerationController(IAdminModerationService moderation) : ControllerBase
{
 [HttpGet("recipes")] public async Task<IActionResult> Recipes([FromQuery] AdminListQuery query, CancellationToken ct) => Ok(await moderation.GetRecipesAsync(query, ct));
 [HttpGet("comments")] public async Task<IActionResult> Comments([FromQuery] AdminListQuery query, CancellationToken ct) => Ok(await moderation.GetCommentsAsync(query, ct));
 [HttpGet("reviews")] public async Task<IActionResult> Reviews([FromQuery] AdminListQuery query, CancellationToken ct) => Ok(await moderation.GetReviewsAsync(query, ct));
}
