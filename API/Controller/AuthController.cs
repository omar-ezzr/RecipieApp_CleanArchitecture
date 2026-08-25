using API.Responses;
using Core.Application.DTO.Auth;
using Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(dto, cancellationToken);

        return result.IsSuccess
            ? Ok(ToTokenResponse(result.Value!))
            : Unauthorized(Error("invalid_credentials", result.Error ?? "Invalid credentials or inactive account"));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(TokenRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshAsync(request, cancellationToken);

        return result.IsSuccess
            ? Ok(ToTokenResponse(result.Value!))
            : Unauthorized(Error("invalid_refresh_token", result.Error ?? "Invalid refresh token"));
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(dto, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok("User created");
        }

        return BadRequest(Error("validation_failed", result.Error ?? "The request is invalid."));
    }

    private static object ToTokenResponse(TokenResponseDto token) => new
    {
        accessToken = token.AccessToken,
        refreshToken = token.RefreshToken
    };

    private ApiErrorResponse Error(string code, string message) => new()
    {
        Code = code,
        Message = message,
        TraceId = HttpContext.TraceIdentifier
    };
}
