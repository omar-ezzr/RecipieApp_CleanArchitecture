namespace Core.Application.DTO.Auth;

public sealed class TokenResponseDto
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}
