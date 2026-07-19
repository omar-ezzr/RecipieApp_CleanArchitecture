using Core.Application.DTO.Auth;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text;

using Core.Application.Interfaces.Services;
using Core.Domain.Constants;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly IConfiguration _configuration;

public AuthController(
    AppDbContext context,
    IPasswordService passwordService,
    IConfiguration configuration)        {
            _context = context;
            _passwordService = passwordService;
            _configuration = configuration;

        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
        {
            var email = NormalizeEmail(dto.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (user == null || !user.IsActive || !_passwordService.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials or inactive account");

     
            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(GetRefreshTokenDays());

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                accessToken,
                refreshToken
            });

        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(TokenRequestDto request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Invalid refresh token",
                    Status = StatusCodes.Status401Unauthorized
                });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

            if (user == null)
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Invalid refresh token",
                    Status = StatusCodes.Status401Unauthorized
                });
            }

            if (!user.IsActive || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _context.SaveChangesAsync(cancellationToken);
                return Unauthorized(new ProblemDetails
                {
                    Title = "Invalid refresh token",
                    Status = StatusCodes.Status401Unauthorized
                });
            }

            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(GetRefreshTokenDays());

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            });
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] LoginDto dto, CancellationToken cancellationToken)
        {
            var email = NormalizeEmail(dto.Email);
            var exists = await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);

            if (exists)
                return BadRequest("User already exists");

            var user = new Users
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = _passwordService.Hash(dto.Password),
                Role = AppRoles.User,
                IsActive = true

            };

            _context.Users.Add(user);
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "User already exists",
                    Status = StatusCodes.Status409Conflict
                });
            }

            return Ok("User created");
        }
      
        private string GenerateJwtToken(Users user)
        {
            var jwtKey = _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException("JWT key is missing from configuration.");
            }

            var accessTokenMinutes = _configuration.GetValue<int>(
                "Jwt:AccessTokenMinutes",
                5
            );

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(accessTokenMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

  private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private int GetRefreshTokenDays()
        {
            var days = _configuration.GetValue<int>("Jwt:RefreshTokenDays", 7);
            return days < 1 ? 7 : days;
        }

        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

    }
}
