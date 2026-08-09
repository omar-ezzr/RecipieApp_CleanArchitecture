using Core.Domain.Enums;

namespace Core.Domain.Entities;

public sealed class Notification
{
    public Guid Id { get; set; }
    public Guid RecipientUserId { get; set; }
    public Users RecipientUser { get; set; } = null!;
    public Guid ActorUserId { get; set; }
    public Users ActorUser { get; set; } = null!;
    public NotificationType Type { get; set; }
    public Guid? RecipeId { get; set; }
    public Recipie? Recipe { get; set; }
    public Guid? CommentId { get; set; }
    public RecipeComment? Comment { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
