using API.Extensions;
using Core.Application.DTO.Cuisines;
using Core.Application.Interfaces.Services;
using Core.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CuisinesController : ControllerBase
{
    private readonly ICuisineService _service;

    public CuisinesController(ICuisineService service)
    {
        _service = service;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAllAsync(cancellationToken));
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : this.ToActionResult(result);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/regions")]
    public async Task<IActionResult> GetRegions(Guid id, CancellationToken cancellationToken)
    {
        var cuisine = await _service.GetByIdAsync(id, cancellationToken);
        if (!cuisine.IsSuccess)
        {
            return this.ToActionResult(cuisine);
        }

        return Ok(await _service.GetRegionsAsync(id, cancellationToken));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCuisineDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCuisineDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : this.ToActionResult(result);
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : this.ToActionResult(result);
    }

}
