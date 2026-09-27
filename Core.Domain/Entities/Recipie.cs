using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Common;
using Core.Domain.Enums;

namespace Core.Domain.Entities
{
    public class Recipie : BaseEntity
    {
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int PreparationTimeMinutes { get; set; }
    public DifficultyLevel Difficulty { get; set; }

    public Guid UserId { get; set; }
    public Users User { get; set; } = default!;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = default!;
    public Guid CuisineId { get; set; }
    public Cuisine Cuisine { get; set; } = default!;
    public Guid? RegionId { get; set; }
    public Region? Region { get; set; }
    public string? TraditionalName { get; set; }
    public string? OriginDescription { get; set; }
    public bool IsTraditional { get; set; }
    public string? ServingOccasion { get; set; }
    public string? ImageUrl { get; set; }
    public ICollection<Ingredient> Ingredients { get; set; } = [];
    public ICollection<RecipieStep> Steps { get; set; } = [];
    public ICollection<RecipeMedia> Media { get; set; } = [];
    
    }
}
