using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Enums;

namespace Core.Application.DTO.Recipe
{
    public class CreateRecipeDto
    {
         public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DifficultyLevel Difficulty { get; set; }
    public int PreparationTimeMinutes { get; set; }
    public Guid CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    }
}