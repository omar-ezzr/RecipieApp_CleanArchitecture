namespace Core.Application.DTO.Reviews
{
    using Core.Application.DTO.Users;

    public class ReviewDto
    {
        public Guid Id { get; set; }

        public Guid RecipeId { get; set; }

        public Guid UserId { get; set; }

        public required AuthorDto Author { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
