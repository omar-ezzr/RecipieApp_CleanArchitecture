using Core.Application.DTO.Users;
using Core.Application.DTO.Recipe;
using Core.Domain.Enums;

public class RecipieDto
{
    public Guid Id { get; set; }

    public required string Title { get; set; }
    public required string Description { get; set; }

    public int PreparationTimeMinutes { get; set; }

    public Guid CategoryId { get; set; }

    public required string Category { get; set; }

    public Guid CuisineId { get; set; }

    public required string CuisineName { get; set; }

    public required string CuisineSlug { get; set; }

    public Guid? RegionId { get; set; }

    public string? RegionName { get; set; }

    public string? RegionSlug { get; set; }

    public required AuthorDto Author { get; set; }

    public string? ImageUrl { get; set; }

    public DifficultyLevel Difficulty { get; set; }

    public string? TraditionalName { get; set; }

    public string? OriginDescription { get; set; }

    public bool IsTraditional { get; set; }

    public string? ServingOccasion { get; set; }

    public int LikeCount { get; set; }

    public bool IsLikedByCurrentUser { get; set; }

    public List<CreateIngredientDto> Ingredients { get; set; } = new();

    public List<CreateRecipeStepDto> Steps { get; set; } = new();
}
