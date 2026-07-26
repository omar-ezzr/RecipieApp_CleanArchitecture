using Core.Domain.Entities;
using Core.Domain.Constants;
using Core.Application.Interfaces.Services;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(
            AppDbContext context,
            IConfiguration configuration,
            IPasswordService passwordService,
            bool isDevelopment,
            CancellationToken cancellationToken = default)
        {
            await SeedAdminAsync(context, configuration, passwordService, isDevelopment, cancellationToken);
            await SeedSystemUserAsync(context, cancellationToken);
            await SeedCategoriesAsync(context, cancellationToken);
            await SeedCuisinesAsync(context, cancellationToken);
            await SeedRegionsAsync(context, cancellationToken);
            await BackfillRecipeCuisineAsync(context, cancellationToken);
            await RecipeSeeder.SeedAsync(context, configuration, isDevelopment, SystemUserId, cancellationToken);
        }

        private static async Task SeedCategoriesAsync(AppDbContext context, CancellationToken cancellationToken)
        {
            var existing = await context.Categories
                .Select(category => category.Name)
                .ToListAsync(cancellationToken);

            var existingSet = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var categories = new[]
            {
                new Category { Id = Guid.NewGuid(), Name = "Breakfast", Description = "Morning meals", CreatedAt = DateTime.UtcNow },
                new Category { Id = Guid.NewGuid(), Name = "Lunch", Description = "Midday meals", CreatedAt = DateTime.UtcNow },
                new Category { Id = Guid.NewGuid(), Name = "Dinner", Description = "Evening meals", CreatedAt = DateTime.UtcNow }
            }
            .Where(category => !existingSet.Contains(category.Name))
            .ToList();

            if (categories.Count > 0)
            {
                await context.Categories.AddRangeAsync(categories, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        private static async Task SeedCuisinesAsync(AppDbContext context, CancellationToken cancellationToken)
        {
            var existingSlugs = await context.Cuisines
                .Select(cuisine => cuisine.Slug)
                .ToListAsync(cancellationToken);

            var existingSet = existingSlugs.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var cuisines = CuisineSeeds
                .Where(cuisine => !existingSet.Contains(cuisine.Slug))
                .Select(cuisine => new Cuisine
                {
                    Id = Guid.NewGuid(),
                    Name = cuisine.Name,
                    Slug = cuisine.Slug,
                    Description = cuisine.Description,
                    CountryCode = cuisine.CountryCode,
                    ImageUrl = cuisine.ImageUrl,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            if (cuisines.Count > 0)
            {
                await context.Cuisines.AddRangeAsync(cuisines, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        private static async Task SeedRegionsAsync(AppDbContext context, CancellationToken cancellationToken)
        {
            var cuisinesBySlug = await context.Cuisines
                .ToDictionaryAsync(cuisine => cuisine.Slug, cuisine => cuisine, StringComparer.OrdinalIgnoreCase, cancellationToken);

            var existing = await context.Regions
                .Select(region => new { region.CuisineId, region.Slug })
                .ToListAsync(cancellationToken);

            var existingKeys = existing
                .Select(region => $"{region.CuisineId:N}:{region.Slug}")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var regions = RegionSeeds
                .Where(seed => cuisinesBySlug.ContainsKey(seed.CuisineSlug))
                .Select(seed => new { Seed = seed, Cuisine = cuisinesBySlug[seed.CuisineSlug] })
                .Where(item => !existingKeys.Contains($"{item.Cuisine.Id:N}:{item.Seed.Slug}"))
                .Select(item => new Region
                {
                    Id = Guid.NewGuid(),
                    Name = item.Seed.Name,
                    Slug = item.Seed.Slug,
                    Description = item.Seed.Description,
                    CuisineId = item.Cuisine.Id,
                    Cuisine = item.Cuisine,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            if (regions.Count > 0)
            {
                await context.Regions.AddRangeAsync(regions, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        private static async Task BackfillRecipeCuisineAsync(AppDbContext context, CancellationToken cancellationToken)
        {
            var internationalId = await context.Cuisines
                .Where(cuisine => cuisine.Slug == "international")
                .Select(cuisine => cuisine.Id)
                .FirstAsync(cancellationToken);

            var recipes = await context.Recipies
                .Where(recipe => recipe.CuisineId == Guid.Empty)
                .ToListAsync(cancellationToken);

            if (recipes.Count == 0)
            {
                return;
            }

            foreach (var recipe in recipes)
            {
                recipe.CuisineId = internationalId;
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedSystemUserAsync(
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            if (await context.Users.AnyAsync(user => user.Id == SystemUserId, cancellationToken))
            {
                return;
            }

            context.Users.Add(new Users
            {
                Id = SystemUserId,
                DisplayName = "Recepie System",
                Email = "system@recepie.local",
                PasswordHash = "SYSTEM_ACCOUNT_NO_LOGIN",
                Role = AppRoles.User,
                IsActive = false
            });

            await context.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedAdminAsync(
            AppDbContext context,
            IConfiguration configuration,
            IPasswordService passwordService,
            bool isDevelopment,
            CancellationToken cancellationToken)
        {
            if (!isDevelopment)
            {
                return;
            }

            var enabled = bool.TryParse(configuration["AdminSeed:Enabled"], out var parsedEnabled)
                && parsedEnabled;

            if (!enabled)
            {
                return;
            }

            var email = configuration["AdminSeed:Email"];
            var password = configuration["AdminSeed:Password"];
            var displayName = configuration["AdminSeed:DisplayName"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            var normalizedEmail = email.Trim().ToLowerInvariant();
            var normalizedDisplayName = string.IsNullOrWhiteSpace(displayName)
                ? "Recepie Admin"
                : displayName.Trim();

            if (normalizedDisplayName.Length > 100)
            {
                normalizedDisplayName = normalizedDisplayName[..100];
            }

            if (await context.Users.AnyAsync(user => user.Email == normalizedEmail, cancellationToken))
            {
                return;
            }

            context.Users.Add(new Users
            {
                Id = Guid.NewGuid(),
                DisplayName = normalizedDisplayName,
                Email = normalizedEmail,
                PasswordHash = passwordService.Hash(password),
                Role = AppRoles.Admin,
                IsActive = true
            });

            await context.SaveChangesAsync(cancellationToken);
        }

        private static readonly Guid SystemUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        private static readonly CuisineSeed[] CuisineSeeds =
        [
            new("Moroccan", "moroccan", "MA", "Tagines, breads, spices, preserved lemons, and regional home cooking.", "https://images.unsplash.com/photo-1541518763669-27fef04b14ea"),
            new("Italian", "italian", "IT", "Pasta, olive oil, regional produce, seafood, and slow family meals.", "https://images.unsplash.com/photo-1498579397066-22750a3cb424"),
            new("Japanese", "japanese", "JP", "Seasonal, precise cooking shaped by rice, seafood, broths, and fermentation.", "https://images.unsplash.com/photo-1611143669185-af224c5e3252"),
            new("Indian", "indian", "IN", "Layered spice traditions, grains, pulses, breads, and regional vegetarian and meat dishes.", "https://images.unsplash.com/photo-1585937421612-70a008356fbe"),
            new("Mexican", "mexican", "MX", "Corn, chiles, beans, herbs, moles, street foods, and regional sauces.", "https://images.unsplash.com/photo-1565299585323-38d6b0865b47"),
            new("French", "french", "FR", "Sauces, pastry, breads, stews, cheeses, and regionally grounded techniques.", "https://images.unsplash.com/photo-1414235077428-338989a2e8c0"),
            new("Turkish", "turkish", "TR", "Anatolian breads, grills, vegetable dishes, meze, and shared table traditions.", "https://images.unsplash.com/photo-1524231757912-21f4fe3a7200"),
            new("Lebanese", "lebanese", "LB", "Levantine mezze, grains, herbs, grilled meats, and bright vegetable dishes.", "https://images.unsplash.com/photo-1542528180-a1208c5169a5"),
            new("International", "international", "XX", "A migration-safe internal cuisine for recipes without a specific cultural origin.", "https://images.unsplash.com/photo-1504674900247-0877df9cc836")
        ];

        private static readonly RegionSeed[] RegionSeeds =
        [
            new("moroccan", "Fes-Meknes", "fes-meknes", "Imperial-city cooking, preserved lemons, and layered spice traditions."),
            new("moroccan", "Souss-Massa", "souss-massa", "Southern Moroccan seafood, argan, amlou, and Amazigh cooking traditions."),
            new("moroccan", "Marrakech-Safi", "marrakech-safi", "Market cooking, tanjia, spices, and Atlantic coastal influences."),
            new("moroccan", "Rif", "rif", "Northern mountain cooking with olives, figs, breads, and Mediterranean influence."),
            new("moroccan", "Sahara", "sahara", "Desert hospitality, dates, grains, milk, and communal dishes."),
            new("italian", "Tuscany", "tuscany", "Olive oil, beans, rustic breads, and restrained seasonal cooking."),
            new("italian", "Sicily", "sicily", "Seafood, citrus, capers, sweets, and Mediterranean crossroads cuisine."),
            new("italian", "Lombardy", "lombardy", "Rice, butter, cheeses, braises, and northern Italian traditions."),
            new("italian", "Campania", "campania", "Tomatoes, mozzarella, pizza, pasta, seafood, and bold southern flavors."),
            new("japanese", "Kansai", "kansai", "Dashi, okonomiyaki, Kyoto vegetables, and refined regional cooking."),
            new("japanese", "Kanto", "kanto", "Tokyo-style broths, sushi traditions, soba, and everyday comfort food."),
            new("japanese", "Hokkaido", "hokkaido", "Dairy, seafood, ramen, potatoes, and northern cold-weather cooking."),
            new("indian", "Punjab", "punjab", "Tandoor breads, legumes, dairy, rich gravies, and wheat-based meals."),
            new("indian", "Gujarat", "gujarat", "Vegetarian thalis, legumes, snacks, and sweet-sour balance."),
            new("indian", "Kerala", "kerala", "Coconut, rice, seafood, spices, and coastal southern cooking."),
            new("indian", "Bengal", "bengal", "Fish, rice, mustard, sweets, and river delta traditions.")
        ];

        private sealed record CuisineSeed(string Name, string Slug, string CountryCode, string Description, string ImageUrl);

        private sealed record RegionSeed(string CuisineSlug, string Name, string Slug, string Description);
    }
}
