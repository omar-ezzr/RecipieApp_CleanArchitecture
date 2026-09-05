using Core.Application.Common;

namespace Core.Application.DTO.Admin;

public sealed class AdminListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
}

public sealed class AdminRecipeListItemDto { public Guid Id { get; init; } public required string Title { get; init; } public required string Author { get; init; } public required string Category { get; init; } public required string Cuisine { get; init; } public DateTime CreatedAt { get; init; } }
public sealed class AdminCommentListItemDto { public Guid Id { get; init; } public Guid RecipeId { get; init; } public required string Content { get; init; } public required string Author { get; init; } public required string RecipeTitle { get; init; } public DateTime CreatedAt { get; init; } }
public sealed class AdminReviewListItemDto { public Guid Id { get; init; } public Guid RecipeId { get; init; } public int Rating { get; init; } public required string Comment { get; init; } public required string Author { get; init; } public required string RecipeTitle { get; init; } public DateTime CreatedAt { get; init; } }
