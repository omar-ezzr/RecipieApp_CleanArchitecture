using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Persistence
{
    public class DataSeeder
    {
            public static async Task SeedAsync(AppDbContext context)
            {
                if (await context.Recipies.AnyAsync())
                {
                    return; // Data already seeded
                }

                  var breakfast = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Breakfast",
            Description = "Morning meals"
        };

        var dinner = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Dinner",
            Description = "Evening meals"
        };

        var recipe1 = new Recipie
        {
            Id = Guid.NewGuid(),
            Title = "Pancakes",
            Description = "Simple homemade pancakes",
            Category = breakfast,
            PreparationTimeMinutes = 15,
            Ingredients =
            [
                new Ingredient { Id = Guid.NewGuid(), Name = "Flour", Quantity = "200g" },
                new Ingredient { Id = Guid.NewGuid(), Name = "Milk", Quantity = "250ml" },
                new Ingredient { Id = Guid.NewGuid(), Name = "Eggs", Quantity = "2" }
            ],
            Steps =
            [
                new RecipieStep { Id = Guid.NewGuid(), StepNumber = 1, Instruction = "Mix ingredients" },
                new RecipieStep { Id = Guid.NewGuid(), StepNumber = 2, Instruction = "Cook in pan" }
            ]
        };

        context.Categories.AddRange(breakfast, dinner);
        context.Recipies.Add(recipe1);

        await context.SaveChangesAsync();
    }
}    }
