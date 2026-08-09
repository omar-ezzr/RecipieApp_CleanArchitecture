namespace Core.Application.DTO.Recipe;

public sealed class RecipeLikeStatsDto
{
    public Guid RecipeId { get; init; }
    public int LikeCount { get; init; }
    public bool IsLikedByCurrentUser { get; init; }
}
