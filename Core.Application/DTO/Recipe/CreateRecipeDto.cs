namespace Core.Application.DTO.Recipe
{
    public class CreateRecipeDto
    {
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string Difficulty { get; set; } = default!;
        public int PreparationTimeMinutes { get; set; }
        public Guid CategoryId { get; set; }
        public string? ImageUrl { get; set; }
    }
}
