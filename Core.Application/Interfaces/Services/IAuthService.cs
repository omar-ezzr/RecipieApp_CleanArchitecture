using Core.Application.Common;
using Core.Application.DTO.Auth;

namespace Core.Application.Interfaces.Services;

public interface IAuthService
{
    Task<ServiceResult<TokenResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<TokenResponseDto>> RefreshAsync(TokenRequestDto request, CancellationToken cancellationToken = default);
    Task<ServiceResult> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
