using Core.Domain.Entities;
using Core.Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // prevent duplicate seeding
            if (await context.Recipies.AnyAsync())
                return;

            // ========================
            // 🧩 CATEGORIES
            // ========================
            var breakfast = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Breakfast",
                Description = "Morning meals",
                CreatedAt = DateTime.UtcNow
            };

            var lunch = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Lunch",
                Description = "Midday meals",
                CreatedAt = DateTime.UtcNow
            };

            var dinner = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Dinner",
                Description = "Evening meals",
                CreatedAt = DateTime.UtcNow
            };

            var categories = new[] { breakfast, lunch, dinner };

            await context.Categories.AddRangeAsync(categories);

            // ========================
            // 🧠 DATA VARIATION
            // ========================
            var keywords = new[]
            {
                "chicken", "pasta", "salad", "beef", "rice", "dessert"
            };

            // ========================
            // 🍽️ RECIPES (1000)
            // ========================
            var recipes = new List<Recipie>();

            for (int i = 1; i <= 1000; i++)
            {
                var category = categories[i % categories.Length];
                var keyword = keywords[i % keywords.Length];

                var recipe = new Recipie
                {
                    Id = Guid.NewGuid(),
                    Title = $"{keyword} recipe {i}",
                    Description = $"Delicious {keyword} dish number {i}",
                    PreparationTimeMinutes = 10 + (i % 60),
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow,

                    // ✅ enum FIX
                    Difficulty = (DifficultyLevel)(i % 3),

                    // ✅ stable image source
                    ImageUrl = $"https://picsum.photos/400/300?random={i}",

                    // ========================
                    // 🧂 INGREDIENTS
                    // ========================
                    Ingredients = new List<Ingredient>
                    {
                        new Ingredient
                        {
                            Id = Guid.NewGuid(),
                            Name = "Salt",
                            Quantity = "1 tsp",
                            CreatedAt = DateTime.UtcNow
                        },
                        new Ingredient
                        {
                            Id = Guid.NewGuid(),
                            Name = keyword,
                            Quantity = "200g",
                            CreatedAt = DateTime.UtcNow
                        }
                    },

                    // ========================
                    // 🪜 STEPS
                    // ========================
                    Steps = new List<RecipieStep>
                    {
                        new RecipieStep
                        {
                            Id = Guid.NewGuid(),
                            Instruction = "Prepare ingredients",
                            StepNumber = 1,
                            CreatedAt = DateTime.UtcNow
                        },
                        new RecipieStep
                        {
                            Id = Guid.NewGuid(),
                            Instruction = "Cook and serve",
                            StepNumber = 2,
                            CreatedAt = DateTime.UtcNow
                        }
                    }
                };

                recipes.Add(recipe);
            }

            // ========================
            // 💾 SAVE ONCE (IMPORTANT)
            // ========================
            await context.Recipies.AddRangeAsync(recipes);
            await context.SaveChangesAsync();
        }
    }
}