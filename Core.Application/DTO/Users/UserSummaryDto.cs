namespace Core.Application.DTO.Users;

public sealed class UserSummaryDto
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public string? CountryCode { get; init; }
    public bool IsFollowing { get; init; }
}
