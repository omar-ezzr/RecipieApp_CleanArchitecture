namespace Core.Application.DTO.Users;

public sealed class PublicUserProfileDto
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? Bio { get; init; }
    public string? AvatarUrl { get; init; }
    public string? CountryCode { get; init; }
    public int FollowerCount { get; init; }
    public int FollowingCount { get; init; }
    public int RecipeCount { get; init; }
}
