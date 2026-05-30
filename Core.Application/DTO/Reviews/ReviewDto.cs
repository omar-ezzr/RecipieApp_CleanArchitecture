namespace Core.Application.DTO.Reviews
{
    public class ReviewDto
    {
        public Guid Id { get; set; }

        public Guid RecipeId { get; set; }

        public Guid UserId { get; set; }

        public string UserEmail { get; set; } = string.Empty;

        public int Rating { get; set; }

        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}