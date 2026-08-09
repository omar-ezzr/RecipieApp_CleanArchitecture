using Core.Application.DTO.Users;
using Core.Domain.Enums;

namespace Core.Application.DTO.Social;

public sealed class FeedRecipeDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public int PreparationTimeMinutes { get; init; }
    public DifficultyLevel Difficulty { get; init; }
    public DateTime CreatedAt { get; init; }
    public required AuthorDto Author { get; init; }
    public required NamedSummaryDto Cuisine { get; init; }
    public NamedSummaryDto? Region { get; init; }
    public bool IsTraditional { get; init; }
    public int LikeCount { get; init; }
    public int CommentCount { get; init; }
    public bool IsLikedByCurrentUser { get; init; }
    public bool IsFavoriteByCurrentUser { get; init; }
}

public sealed class NamedSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
