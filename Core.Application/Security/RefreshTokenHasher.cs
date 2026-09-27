using System.Security.Cryptography;
using System.Text;

namespace Core.Application.Security;

public static class RefreshTokenHasher
{
    public static string Hash(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("Refresh token must not be empty.", nameof(refreshToken));
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
    }
}
