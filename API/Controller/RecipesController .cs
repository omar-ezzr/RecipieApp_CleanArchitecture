using Core.Application.DTO.Recipe;
using Core.Application.DTO;
using Core.Application.Interfaces.Services;
using Core.Domain.Constants;
using Core.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Core.Application.Common;
using API.Extensions;
using API.Responses;
namespace API.Controllers;

[ApiController]

[Authorize]
[Route("api/[controller]")]
public class RecipesController : ControllerBase
{
    private readonly IRecipeService _service;

    public RecipesController(IRecipeService service)
    {
        _service = service;
    }

    // GET: api/recipes
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var currentUserId = TryGetCurrentUserId(out var parsed) ? parsed : (Guid?)null;
        var recipes = await _service.GetPagedAsync(
            new RecipeQueryParams { Page = 1, PageSize = 100 },
            currentUserId,
            cancellationToken);

        return Ok(recipes);
    }

    [Authorize]
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] RecipeQueryParams parameters,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(parameters.Difficulty) &&
            (!Enum.TryParse<DifficultyLevel>(parameters.Difficulty, true, out var difficulty) ||
             !Enum.IsDefined(typeof(DifficultyLevel), difficulty)))
        {
            return BadRequest(Error("validation_failed", "Difficulty must be Easy, Medium, or Hard."));
        }

        var currentUserId = TryGetCurrentUserId(out var parsed) ? parsed : (Guid?)null;
        var result = await _service.GetPagedAsync(parameters, currentUserId, cancellationToken);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMine(
        [FromQuery] RecipeQueryParams parameters,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return UnauthorizedIdentityProblem();
        }

        if (!IsValidDifficultyFilter(parameters.Difficulty))
        {
            return BadRequest(Error("validation_failed", "Difficulty must be Easy, Medium, or Hard."));
        }

        var result = await _service.GetMineAsync(parameters, currentUserId, cancellationToken);

        return Ok(result);
    }

    // GET: api/recipes/{id}
    [Authorize]
    [HttpGet("{id}")]

    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = TryGetCurrentUserId(out var parsed) ? parsed : (Guid?)null;
        var recipe = await _service.GetByIdAsync(id, currentUserId, cancellationToken);

        if (recipe == null)
            return NotFound();

        return Ok(recipe);
    }

    // POST: api/recipes
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecipeDto dto, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return UnauthorizedIdentityProblem();
        }

        var result = await _service.CreateAsync(dto, currentUserId, cancellationToken);

        if (!result.IsSuccess)
            return ToActionResult(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }
    [Authorize]

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id,[FromBody] CreateRecipeDto dto, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return UnauthorizedIdentityProblem();
        }

        var result = await _service.UpdateAsync(id, dto, currentUserId, IsAdmin(), cancellationToken);
        if (!result.IsSuccess)
        return ToActionResult(result);
        return Ok(result.Value);
    }
    [HttpPost("{id}/media")]
    [RequestSizeLimit(53 * 1024 * 1024)]
    public async Task<IActionResult> AddMedia(Guid id, IFormFile? file, CancellationToken cancellationToken)
    { if (!TryGetCurrentUserId(out var userId)) return UnauthorizedIdentityProblem(); if (file is null) return BadRequest(Error("invalid_media", "A media file is required.")); await using var stream=file.OpenReadStream(); var result=await _service.AddMediaAsync(id,stream,file.FileName,file.ContentType,file.Length,userId,IsAdmin(),cancellationToken); return result.IsSuccess?Ok(result.Value):ToActionResult(result); }
    [HttpDelete("{id}/media/{mediaId}")]
    public async Task<IActionResult> RemoveMedia(Guid id, Guid mediaId, CancellationToken cancellationToken)
    { if (!TryGetCurrentUserId(out var userId)) return UnauthorizedIdentityProblem(); var result=await _service.RemoveMediaAsync(id,mediaId,userId,IsAdmin(),cancellationToken);return result.IsSuccess?NoContent():ToActionResult(result); }
    [HttpPut("{id}/media/{mediaId}/main")]
    public async Task<IActionResult> SetMainMedia(Guid id, Guid mediaId, CancellationToken cancellationToken)
    { if (!TryGetCurrentUserId(out var userId)) return UnauthorizedIdentityProblem(); var result=await _service.SetMainMediaAsync(id,mediaId,userId,IsAdmin(),cancellationToken);return result.IsSuccess?NoContent():ToActionResult(result); }
    [HttpPut("{id}/media/order")]
    public async Task<IActionResult> ReorderMedia(Guid id, [FromBody] ReorderRecipeMediaDto dto, CancellationToken cancellationToken)
    { if (!TryGetCurrentUserId(out var userId)) return UnauthorizedIdentityProblem(); var result=await _service.ReorderMediaAsync(id,dto.MediaIds,userId,IsAdmin(),cancellationToken);return result.IsSuccess?NoContent():ToActionResult(result); }

    [Authorize]

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return UnauthorizedIdentityProblem();
        }

        var result = await _service.DeleteAsync(id, currentUserId, IsAdmin(), cancellationToken);

        if (!result.IsSuccess)
            return ToActionResult(result);

        return NoContent();
    }

    private bool TryGetCurrentUserId(out Guid currentUserId)
    {
        return User.TryGetCurrentUserId(out currentUserId);
    }

    private bool IsAdmin() => User.IsInRole(AppRoles.Admin);

    private IActionResult ToActionResult(ServiceResult result)
    {
        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(Error("recipe_not_found", "Recipe was not found.")),
            ServiceErrorType.Forbidden => Forbid(),
            ServiceErrorType.Validation => BadRequest(Error("validation_failed", result.Error ?? "The request is invalid.")),
            ServiceErrorType.Conflict => Conflict(Error("conflict", result.Error ?? "Conflict.")),
            _ => BadRequest(Error("bad_request", result.Error ?? "The request is invalid."))
        };
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(Error("recipe_not_found", "Recipe was not found.")),
            ServiceErrorType.Forbidden => Forbid(),
            ServiceErrorType.Validation => BadRequest(Error("validation_failed", result.Error ?? "The request is invalid.")),
            ServiceErrorType.Conflict => Conflict(Error("conflict", result.Error ?? "Conflict.")),
            _ => BadRequest(Error("bad_request", result.Error ?? "The request is invalid."))
        };
    }

    private UnauthorizedObjectResult UnauthorizedIdentityProblem() =>
        Unauthorized(Error("invalid_identity", "Missing or malformed user identity claim"));

    private ApiErrorResponse Error(string code, string message) =>
        new()
        {
            Code = code,
            Message = message,
            TraceId = HttpContext.TraceIdentifier
        };

    private static bool IsValidDifficultyFilter(string? difficulty)
    {
        return string.IsNullOrWhiteSpace(difficulty)
            || (Enum.TryParse<DifficultyLevel>(difficulty, true, out var parsed)
                && Enum.IsDefined(typeof(DifficultyLevel), parsed));
    }
}
