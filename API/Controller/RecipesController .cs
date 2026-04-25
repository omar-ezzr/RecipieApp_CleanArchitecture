using Core.Application.DTO.Recipe;
using Core.Application.DTO;
using Core.Application.Interfaces.Services;
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
    public async Task<IActionResult> Get()
    {
        var recipes = await _service.GetAllAsync();
        return Ok(recipes);
    }

    [Authorize]
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] RecipeQueryParams parameters)
    {
        var (recipes, total) = await _service.GetPagedAsync(parameters);

        return Ok(new
        {
            Items = recipes,
            Total = total,
            parameters.Page,
            parameters.PageSize
        });
    }

    // GET: api/recipes/{id}
    [Authorize]
    [HttpGet("{id}")]

    public async Task<IActionResult> GetById(Guid id)
    {
        var recipe = await _service.GetByIdAsync(id);

        if (recipe == null)
            return NotFound();

        return Ok(recipe);
    }

    // POST: api/recipes
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create( [FromBody] CreateRecipeDto dto)
    {
        var result = await _service.CreateAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok();
    }
    [Authorize(Roles = "Admin")]

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id,[FromBody] CreateRecipeDto dto)
    {

        var result = await _service.UpdateAsync(id, dto);
        if (!result.IsSuccess)
        return BadRequest(result.Error);
        return Ok();
    }
    [Authorize(Roles = "Admin")]

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result.IsSuccess)
            return NotFound(result.Error);

        return Ok();
    }
}
