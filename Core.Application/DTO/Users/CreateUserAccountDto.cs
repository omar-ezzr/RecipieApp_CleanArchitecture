namespace Core.Application.DTO.Users;

public sealed class CreateUserAccountDto
{
    public string? DisplayName { get; set; }
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string Role { get; set; } = default!;
}
