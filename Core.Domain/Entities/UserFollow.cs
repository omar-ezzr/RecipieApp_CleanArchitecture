namespace Core.Domain.Entities;

public sealed class UserFollow
{
    public Guid Id { get; set; }
    public Guid FollowerUserId { get; set; }
    public Users FollowerUser { get; set; } = null!;
    public Guid FollowedUserId { get; set; }
    public Users FollowedUser { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
