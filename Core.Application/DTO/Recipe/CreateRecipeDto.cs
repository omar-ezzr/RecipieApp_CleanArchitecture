using Core.Domain.Enums;

namespace Core.Application.DTO.Recipe
{
    public class CreateRecipeDto
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public int PreparationTimeMinutes { get; set; }
        public DifficultyLevel Difficulty { get; set; }
        public Guid CategoryId { get; set; }
        public Guid CuisineId { get; set; }
        public Guid? RegionId { get; set; }
        public string? ImageUrl { get; set; }
        public string? TraditionalName { get; set; }
        public string? OriginDescription { get; set; }
        public bool IsTraditional { get; set; }
        public string? ServingOccasion { get; set; }
        public IReadOnlyCollection<CreateIngredientDto> Ingredients { get; set; } = [];
        public IReadOnlyCollection<CreateRecipeStepDto> Steps { get; set; } = [];
    }
}
