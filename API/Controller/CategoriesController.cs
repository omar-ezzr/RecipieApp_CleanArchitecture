using Core.Application.Interfaces.Services;
using Core.Application.DTO.Categories;
using Core.Application.Common;
using Core.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) => Ok(await _service.GetAllAsync(cancellationToken));

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto, CancellationToken cancellationToken) { var result = await _service.CreateAsync(dto, cancellationToken); return result.IsSuccess ? Ok(result.Value) : Error(result); }
    [Authorize(Roles = AppRoles.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto dto, CancellationToken cancellationToken) { var result = await _service.UpdateAsync(id, dto, cancellationToken); return result.IsSuccess ? Ok(result.Value) : Error(result); }
    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) { var result = await _service.DeleteAsync(id, cancellationToken); return result.IsSuccess ? NoContent() : Error(result); }
    private IActionResult Error(ServiceResult result) => result.ErrorType == ServiceErrorType.NotFound ? NotFound(new { message = result.Error }) : result.ErrorType == ServiceErrorType.Conflict ? Conflict(new { message = result.Error }) : BadRequest(new { message = result.Error });
    private IActionResult Error<T>(ServiceResult<T> result) => result.ErrorType == ServiceErrorType.NotFound ? NotFound(new { message = result.Error }) : result.ErrorType == ServiceErrorType.Conflict ? Conflict(new { message = result.Error }) : BadRequest(new { message = result.Error });
}
