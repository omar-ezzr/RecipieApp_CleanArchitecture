namespace Core.Application.DTO.Users;

public sealed class UserQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
}
