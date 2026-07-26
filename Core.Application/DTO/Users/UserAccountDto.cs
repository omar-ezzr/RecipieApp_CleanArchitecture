namespace Core.Application.DTO.Users;

public sealed class UserAccountDto
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string Role { get; init; } = default!;
    public bool IsActive { get; init; }
}
