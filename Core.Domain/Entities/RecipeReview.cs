namespace Core.Domain.Entities
{
    public class RecipeReview
    {
        public Guid Id { get; set; }

        public Guid RecipeId { get; set; }
        public Recipie Recipe { get; set; } = null!;

        public Guid UserId { get; set; }
        public Users User { get; set; } = null!;

        public int Rating { get; set; }

        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}