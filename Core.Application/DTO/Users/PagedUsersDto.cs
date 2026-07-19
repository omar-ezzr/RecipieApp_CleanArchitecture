namespace Core.Application.DTO.Users;

public sealed class PagedUsersDto
{
    public IReadOnlyCollection<UserAccountDto> Items { get; init; } = [];
    public int Total { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}
