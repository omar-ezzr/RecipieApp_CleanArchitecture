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

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public ICollection<Ingredient> Ingredients { get; set; } = [];
    public ICollection<RecipieStep> Steps { get; set; } = [];
    public ICollection<RecipeImage> Images { get; set; } = [];
    
    }
}
