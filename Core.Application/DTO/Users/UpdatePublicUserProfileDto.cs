namespace Core.Application.DTO.Users;

public sealed class UpdatePublicUserProfileDto
{
    public string DisplayName { get; init; } = string.Empty;
    public string? Bio { get; init; }
    public string? AvatarUrl { get; init; }
    public string? CountryCode { get; init; }
}
