namespace Core.Domain.Constants;

public static class AppRoles
{
    public const string User = "User";
    public const string Operator = "Operator";
    public const string Admin = "Admin";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        User,
        Operator,
        Admin
    };

    public static bool TryNormalize(string? role, out string normalizedRole)
    {
        normalizedRole = string.Empty;

        if (string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        var trimmed = role.Trim();

        if (trimmed.Equals(User, StringComparison.OrdinalIgnoreCase))
        {
            normalizedRole = User;
            return true;
        }

        if (trimmed.Equals(Operator, StringComparison.OrdinalIgnoreCase))
        {
            normalizedRole = Operator;
            return true;
        }

        if (trimmed.Equals(Admin, StringComparison.OrdinalIgnoreCase))
        {
            normalizedRole = Admin;
            return true;
        }

        return false;
    }

    public static bool IsSupported(string? role)
    {
        return TryNormalize(role, out _);
    }
}
