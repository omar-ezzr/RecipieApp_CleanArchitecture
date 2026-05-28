using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.DTO.Favorites
{
    public class FavoriteRecipeDto
    {
         public Guid Id { get; set; }
        public Guid RecipeId { get; set; }
        public string RecipeTitle { get; set; } = string.Empty;
        public string? RecipeImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}