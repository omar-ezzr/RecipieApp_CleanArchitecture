namespace Core.Domain.Entities;

public sealed class RecipeLike
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Users User { get; set; } = null!;
    public Guid RecipeId { get; set; }
    public Recipie Recipe { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
