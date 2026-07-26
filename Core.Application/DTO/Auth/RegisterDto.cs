namespace Core.Application.DTO.Auth;

public class RegisterDto
{
    public string DisplayName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}
