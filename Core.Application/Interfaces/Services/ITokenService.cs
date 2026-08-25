using Core.Domain.Entities;

namespace Core.Application.Interfaces.Services;

public interface ITokenService
{
    string CreateAccessToken(Users user);
    string CreateRefreshToken();
    int GetRefreshTokenDays();
}
