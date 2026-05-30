namespace Core.Application.DTO.Reviews
{
    public class UpdateReviewDto
    {
        public int Rating { get; set; }

        public string Comment { get; set; } = string.Empty;
    }
}