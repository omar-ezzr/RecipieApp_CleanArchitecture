using Core.Application.DTO.Recipe;
using Core.Application.DTO;
using Core.Application.Interfaces.Services;
using Core.Domain.Constants;
using Core.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
        var recipes = await _service.GetPagedAsync(
            new RecipeQueryParams { Page = 1, PageSize = 100 },
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
            ModelState.AddModelError("difficulty", "Difficulty must be Easy, Medium, or Hard.");
            return ValidationProblem(ModelState);
        }

        var result = await _service.GetPagedAsync(parameters, cancellationToken);

        return Ok(result);
    }

    // GET: api/recipes/{id}
    [Authorize]
    [HttpGet("{id}")]

    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var recipe = await _service.GetByIdAsync(id, cancellationToken);

        if (recipe == null)
            return NotFound();

        return Ok(recipe);
    }

    // POST: api/recipes
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Operator}")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecipeDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok();
    }
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Operator}")]

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id,[FromBody] CreateRecipeDto dto, CancellationToken cancellationToken)
    {

        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        if (!result.IsSuccess)
        return BadRequest(result.Error);
        return Ok();
    }
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Operator}")]

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(result.Error);

        return Ok();
    }
}
