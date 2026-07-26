using API.Responses;
using Core.Application.Common;
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

        return result.IsSuccess ? Ok(result.Value) : ToActionResult(result);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/regions")]
    public async Task<IActionResult> GetRegions(Guid id, CancellationToken cancellationToken)
    {
        var cuisine = await _service.GetByIdAsync(id, cancellationToken);
        if (!cuisine.IsSuccess)
        {
            return ToActionResult(cuisine);
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
            return ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCuisineDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ToActionResult(result);
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : ToActionResult(result);
    }

    private IActionResult ToActionResult(ServiceResult result)
    {
        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(Error("not_found", result.Error ?? "Resource was not found.")),
            ServiceErrorType.Conflict => Conflict(Error("conflict", result.Error ?? "Conflict.")),
            ServiceErrorType.Validation => BadRequest(Error("validation_failed", result.Error ?? "The request is invalid.")),
            _ => BadRequest(Error("bad_request", result.Error ?? "The request is invalid."))
        };
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(Error("not_found", result.Error ?? "Resource was not found.")),
            ServiceErrorType.Conflict => Conflict(Error("conflict", result.Error ?? "Conflict.")),
            ServiceErrorType.Validation => BadRequest(Error("validation_failed", result.Error ?? "The request is invalid.")),
            _ => BadRequest(Error("bad_request", result.Error ?? "The request is invalid."))
        };
    }

    private ApiErrorResponse Error(string code, string message) =>
        new()
        {
            Code = code,
            Message = message,
            TraceId = HttpContext.TraceIdentifier
        };
}
