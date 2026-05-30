namespace Core.Application.DTO.Reviews
{
    public class CreateReviewDto
    {
        public Guid RecipeId { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; } = string.Empty;
    }
}