using API.Extensions;
using Core.Application.DTO.Social;
using Core.Application.Interfaces.Services;
using Core.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller;

[ApiController]
public sealed class RecipeCommentsController : ControllerBase
{
    private readonly IRecipeCommentService _commentService;

    public RecipeCommentsController(IRecipeCommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("api/recipes/{recipeId:guid}/comments")]
    public async Task<IActionResult> GetByRecipe(Guid recipeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _commentService.GetByRecipeAsync(recipeId, page, pageSize, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToActionResult(result);
    }

    [Authorize]
    [HttpPost("api/recipes/{recipeId:guid}/comments")]
    public async Task<IActionResult> Create(Guid recipeId, [FromBody] CreateRecipeCommentDto dto, CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return this.UnauthorizedIdentityProblem();
        }

        var result = await _commentService.CreateAsync(currentUserId, recipeId, dto, cancellationToken);
        return result.IsSuccess ? Created($"api/comments/{result.Value!.Id}", result.Value) : this.ToActionResult(result);
    }

    [Authorize]
    [HttpPut("api/comments/{commentId:guid}")]
    public async Task<IActionResult> Update(Guid commentId, [FromBody] UpdateRecipeCommentDto dto, CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return this.UnauthorizedIdentityProblem();
        }

        var result = await _commentService.UpdateAsync(currentUserId, commentId, dto, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToActionResult(result);
    }

    [Authorize]
    [HttpDelete("api/comments/{commentId:guid}")]
    public async Task<IActionResult> Delete(Guid commentId, CancellationToken cancellationToken)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
        {
            return this.UnauthorizedIdentityProblem();
        }

        var result = await _commentService.DeleteAsync(currentUserId, User.IsInRole(AppRoles.Admin), commentId, cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToActionResult(result);
    }

}
