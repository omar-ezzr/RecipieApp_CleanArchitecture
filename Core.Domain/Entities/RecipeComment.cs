namespace Core.Domain.Entities;

public sealed class RecipeComment
{
    public Guid Id { get; set; }
    public Guid RecipeId { get; set; }
    public Recipie Recipe { get; set; } = null!;
    public Guid UserId { get; set; }
    public Users User { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
