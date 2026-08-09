using Core.Application.DTO.Users;

namespace Core.Application.DTO.Social;

public sealed class RecipeCommentDto
{
    public Guid Id { get; init; }
    public Guid RecipeId { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public required AuthorDto Author { get; init; }
}

public sealed class CreateRecipeCommentDto
{
    public string Content { get; init; } = string.Empty;
}

public sealed class UpdateRecipeCommentDto
{
    public string Content { get; init; } = string.Empty;
}
