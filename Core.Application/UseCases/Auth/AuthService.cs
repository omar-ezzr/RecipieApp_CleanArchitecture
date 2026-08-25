using Core.Application.Common;
using Core.Application.DTO.Auth;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Domain.Constants;
using DomainUser = Core.Domain.Entities.Users;

namespace Core.Application.UseCases.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository users, IPasswordService passwordService, ITokenService tokenService)
    {
        _users = users;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<ServiceResult<TokenResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(dto.Email);
        var user = await _users.GetByEmailAsync(email, track: true, cancellationToken);

        if (user is null || !user.IsActive || !_passwordService.Verify(dto.Password, user.PasswordHash))
        {
            return ServiceResult<TokenResponseDto>.Failure("Invalid credentials or inactive account", ServiceErrorType.Forbidden);
        }

        var tokens = RotateTokens(user);
        await _users.SaveChangesAsync(cancellationToken);

        return ServiceResult<TokenResponseDto>.Success(tokens);
    }

    public async Task<ServiceResult<TokenResponseDto>> RefreshAsync(TokenRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return InvalidRefreshToken();
        }

        var user = await _users.GetByRefreshTokenAsync(request.RefreshToken, track: true, cancellationToken);

        if (user is null)
        {
            return InvalidRefreshToken();
        }

        if (!user.IsActive || user.RefreshTokenExpiryTime < DateTime.UtcNow)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _users.SaveChangesAsync(cancellationToken);

            return InvalidRefreshToken();
        }

        var tokens = RotateTokens(user);
        await _users.SaveChangesAsync(cancellationToken);

        return ServiceResult<TokenResponseDto>.Success(tokens);
    }

    public async Task<ServiceResult> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        var displayName = dto.DisplayName?.Trim();

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return ServiceResult.Failure("Display name is required", ServiceErrorType.Validation);
        }

        if (displayName.Length > 100)
        {
            return ServiceResult.Failure("Display name must be 100 characters or fewer", ServiceErrorType.Validation);
        }

        var email = NormalizeEmail(dto.Email);

        if (await _users.EmailExistsAsync(email, cancellationToken))
        {
            return ServiceResult.Failure("User already exists", ServiceErrorType.Validation);
        }

        var user = new DomainUser
        {
            Id = Guid.NewGuid(),
            DisplayName = displayName,
            Email = email,
            PasswordHash = _passwordService.Hash(dto.Password),
            Role = AppRoles.User,
            IsActive = true
        };

        await _users.AddAsync(user, cancellationToken);
        await _users.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    private TokenResponseDto RotateTokens(DomainUser user)
    {
        var accessToken = _tokenService.CreateAccessToken(user);
        var refreshToken = _tokenService.CreateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_tokenService.GetRefreshTokenDays());

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    private static ServiceResult<TokenResponseDto> InvalidRefreshToken()
    {
        return ServiceResult<TokenResponseDto>.Failure("Invalid refresh token", ServiceErrorType.Forbidden);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
