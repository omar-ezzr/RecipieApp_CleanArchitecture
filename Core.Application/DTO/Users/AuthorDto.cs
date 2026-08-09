namespace Core.Application.DTO.Users;

public sealed class AuthorDto
{
    public Guid Id { get; set; }
    public required string DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
}
