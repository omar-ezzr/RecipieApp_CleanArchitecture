using Core.Application.DTO.Auth;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text;

using Core.Application.Interfaces.Services;
using Core.Domain.Entities;

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
        public IActionResult Login([FromBody] LoginDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);

            if (user == null || !_passwordService.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

     
            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            _context.SaveChanges();

            return Ok(new
            {
                accessToken,
                refreshToken
            });

        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public IActionResult Refresh(TokenRequestDto request)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.RefreshToken == request.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return Unauthorized("Invalid refresh token");

            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            _context.SaveChanges();

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            });
        }
        [HttpPost("register")]
        public IActionResult Register([FromBody] LoginDto dto)
        {
            var exists = _context.Users.Any(u => u.Email == dto.Email);

            if (exists)
                return BadRequest("User already exists");

            var user = new Users
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                PasswordHash = _passwordService.Hash(dto.Password),
                Role = "User" // default

            };

            _context.Users.Add(user);
            _context.SaveChanges();

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


    }
}