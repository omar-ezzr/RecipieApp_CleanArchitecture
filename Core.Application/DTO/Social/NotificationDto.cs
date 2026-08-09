using Core.Application.DTO.Users;

namespace Core.Application.DTO.Social;

public sealed class NotificationDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public required AuthorDto Actor { get; init; }
    public Guid? RecipeId { get; init; }
    public string? RecipeTitle { get; init; }
    public Guid? CommentId { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class UnreadNotificationCountDto
{
    public int Count { get; init; }
}
