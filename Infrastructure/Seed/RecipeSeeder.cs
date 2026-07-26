using Core.Domain.Entities;
using Core.Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Seed;

public static class RecipeSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        IConfiguration configuration,
        bool isDevelopment,
        Guid systemUserId,
        CancellationToken cancellationToken = default)
    {
        var options = new RecipeSeedOptions
        {
            ResetRecipes = ReadBoolean(configuration, "ResetRecipes", defaultValue: false),
            SeedRealRecipes = ReadBoolean(configuration, "SeedRealRecipes", defaultValue: true)
        };

        if (options.ResetRecipes)
        {
            if (!isDevelopment)
            {
                throw new InvalidOperationException("Recipe reset is allowed only in Development.");
            }

            await ResetRecipeDataAsync(context, cancellationToken);
        }

        if (!options.SeedRealRecipes)
        {
            return;
        }

        var systemUserExists = await context.Users
            .AsNoTracking()
            .AnyAsync(user => user.Id == systemUserId, cancellationToken);

        if (!systemUserExists)
        {
            throw new InvalidOperationException("System user is required before recipe seeding.");
        }

        var definitions = SeedDefinitions();
        var existingTitles = await context.Recipies
            .AsNoTracking()
            .Select(recipe => recipe.Title)
            .ToListAsync(cancellationToken);

        var existingTitleSet = existingTitles
            .Select(NormalizeKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missingDefinitions = definitions
            .Where(recipe => !existingTitleSet.Contains(NormalizeKey(recipe.Title)))
            .ToList();

        if (missingDefinitions.Count == 0)
        {
            return;
        }

        var categoriesByName = await context.Categories
            .ToDictionaryAsync(category => category.Name, category => category, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var cuisinesBySlug = await context.Cuisines
            .Where(cuisine => cuisine.IsActive)
            .ToDictionaryAsync(cuisine => cuisine.Slug, cuisine => cuisine, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var regions = await context.Regions
            .AsNoTracking()
            .Where(region => region.IsActive)
            .Include(region => region.Cuisine)
            .ToListAsync(cancellationToken);

        var regionsByCuisineAndSlug = regions.ToDictionary(
            region => RegionKey(region.Cuisine.Slug, region.Slug),
            region => region,
            StringComparer.OrdinalIgnoreCase);

        var now = DateTime.UtcNow;
        var recipes = new List<Recipie>();

        foreach (var definition in missingDefinitions)
        {
            if (!categoriesByName.TryGetValue(definition.CategoryName, out var category))
            {
                throw new InvalidOperationException($"Recipe seed category '{definition.CategoryName}' was not found.");
            }

            if (!cuisinesBySlug.TryGetValue(definition.CuisineSlug, out var cuisine))
            {
                throw new InvalidOperationException($"Recipe seed cuisine '{definition.CuisineSlug}' was not found.");
            }

            Guid? regionId = null;
            if (!string.IsNullOrWhiteSpace(definition.RegionSlug))
            {
                var key = RegionKey(definition.CuisineSlug, definition.RegionSlug);
                if (!regionsByCuisineAndSlug.TryGetValue(key, out var region))
                {
                    throw new InvalidOperationException(
                        $"Recipe seed region '{definition.RegionSlug}' was not found for cuisine '{definition.CuisineSlug}'.");
                }

                if (region.CuisineId != cuisine.Id)
                {
                    throw new InvalidOperationException(
                        $"Recipe seed region '{definition.RegionSlug}' does not belong to cuisine '{definition.CuisineSlug}'.");
                }

                regionId = region.Id;
            }

            recipes.Add(new Recipie
            {
                Id = Guid.NewGuid(),
                Title = definition.Title,
                Description = definition.Description,
                PreparationTimeMinutes = definition.PreparationTimeMinutes,
                Difficulty = definition.Difficulty,
                CategoryId = category.Id,
                UserId = systemUserId,
                CuisineId = cuisine.Id,
                RegionId = regionId,
                ImageUrl = definition.ImageUrl,
                TraditionalName = definition.TraditionalName,
                OriginDescription = definition.OriginDescription,
                IsTraditional = definition.IsTraditional,
                ServingOccasion = definition.ServingOccasion,
                CreatedAt = now,
                Ingredients = definition.Ingredients
                    .Select(ingredient => new Ingredient
                    {
                        Id = Guid.NewGuid(),
                        Name = ingredient.Name,
                        Quantity = ingredient.Quantity,
                        CreatedAt = now
                    })
                    .ToList(),
                Steps = definition.Steps
                    .Select((step, index) => new RecipieStep
                    {
                        Id = Guid.NewGuid(),
                        StepNumber = index + 1,
                        Instruction = step,
                        CreatedAt = now
                    })
                    .ToList()
            });
        }

        await context.Recipies.AddRangeAsync(recipes, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        Console.WriteLine(
            $"Recipe seed inserted: recipes={recipes.Count}, ingredients={recipes.Sum(recipe => recipe.Ingredients.Count)}, steps={recipes.Sum(recipe => recipe.Steps.Count)}");
    }

    private static async Task ResetRecipeDataAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var reviewsDeleted = await context.RecipeReviews.ExecuteDeleteAsync(cancellationToken);
        var favoritesDeleted = await context.FavoriteRecipes.ExecuteDeleteAsync(cancellationToken);
        var imagesDeleted = await context.Set<RecipeImage>().ExecuteDeleteAsync(cancellationToken);
        var stepsDeleted = await context.RecipeSteps.ExecuteDeleteAsync(cancellationToken);
        var ingredientsDeleted = await context.Ingredients.ExecuteDeleteAsync(cancellationToken);
        var recipesDeleted = await context.Recipies.ExecuteDeleteAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        Console.WriteLine(
            "Recipe seed reset removed: " +
            $"recipes={recipesDeleted}, reviews={reviewsDeleted}, favorites={favoritesDeleted}, " +
            $"recipeImages={imagesDeleted}, steps={stepsDeleted}, ingredients={ingredientsDeleted}");
    }

    private static string NormalizeKey(string value) => value.Trim().ToUpperInvariant();

    private static bool ReadBoolean(IConfiguration configuration, string key, bool defaultValue)
    {
        var value = configuration[$"{RecipeSeedOptions.SectionName}:{key}"];

        return bool.TryParse(value, out var parsed) ? parsed : defaultValue;
    }

    private static string RegionKey(string cuisineSlug, string regionSlug) =>
        $"{cuisineSlug.Trim().ToLowerInvariant()}:{regionSlug.Trim().ToLowerInvariant()}";

    private static IReadOnlyCollection<RecipeSeedDefinition> SeedDefinitions() =>
    [
        new(
            "Moroccan Chicken Tagine with Preserved Lemon",
            "A slow-cooked Moroccan chicken tagine layered with softened onions, preserved lemon, green olives, fresh herbs, ginger, turmeric, and saffron. The sauce is reduced until glossy so the chicken carries the bright, savory flavor of the tagine pot.",
            "Dinner",
            "moroccan",
            "marrakech-safi",
            "/images/recipes/moroccan-chicken-tagine.jpeg",
            DifficultyLevel.Medium,
            90,
            "طاجين الدجاج بالحامض المصير والزيتون",
            "Chicken with preserved lemon and olives is a familiar Moroccan family dish, especially in Marrakech-Safi homes where slow tagine cooking builds a concentrated onion, herb, and spice sauce.",
            true,
            "Family lunch or dinner",
            [
                new("Chicken pieces", "1.5 kg"),
                new("Preserved lemon", "1, quartered"),
                new("Green olives", "1 cup"),
                new("Onion", "2 large, sliced"),
                new("Garlic", "4 cloves, minced"),
                new("Fresh coriander", "1/2 bunch, chopped"),
                new("Fresh parsley", "1/2 bunch, chopped"),
                new("Ground ginger", "2 tsp"),
                new("Turmeric", "1 tsp"),
                new("Saffron or saffron water", "1 pinch"),
                new("Olive oil", "4 tbsp"),
                new("Salt", "to taste"),
                new("Black pepper", "1 tsp"),
                new("Water", "1 cup")
            ],
            [
                "Pat the chicken dry, then season it with salt, pepper, ginger, turmeric, saffron water, garlic, parsley, coriander, and olive oil.",
                "Spread the onions in the base of a tagine or heavy pot and place the seasoned chicken over them with any remaining marinade.",
                "Add water, cover, and cook gently until the chicken is tender and the onions have melted into the sauce.",
                "Add green olives and preserved lemon, then simmer uncovered so the citrus and briny flavors infuse the chicken.",
                "Reduce the sauce until glossy, spoon it over the chicken, and serve hot with bread or couscous."
            ]),
        new(
            "Moroccan Seven-Vegetable Couscous",
            "A generous Friday-style couscous with steamed semolina, chickpeas, and vegetables cooked in a fragrant turmeric and ginger broth. The vegetables are arranged over the couscous and served with extra broth at the table.",
            "Lunch",
            "moroccan",
            "fes-meknes",
            "/images/recipes/moroccan-seven-vegetable-couscous.jpg",
            DifficultyLevel.Hard,
            120,
            "كسكس بسبع خضاري",
            "Seven-vegetable couscous is associated with Moroccan Friday family lunches and regional home cooking around imperial cities such as Fes and Meknes.",
            true,
            "Friday family lunch",
            [
                new("Couscous semolina", "500 g"),
                new("Chickpeas", "1 cup, soaked"),
                new("Carrots", "3, halved"),
                new("Zucchini", "3, halved"),
                new("Pumpkin", "400 g"),
                new("Turnips", "2, quartered"),
                new("Cabbage", "1/2 small head"),
                new("Tomatoes", "2, grated"),
                new("Onion", "1 large, chopped"),
                new("Fresh coriander", "1 bunch"),
                new("Olive oil", "3 tbsp"),
                new("Ginger", "2 tsp"),
                new("Turmeric", "1 tsp"),
                new("Black pepper", "1 tsp"),
                new("Salt", "to taste"),
                new("Broth or water", "2.5 liters")
            ],
            [
                "Start the broth with onion, tomatoes, olive oil, chickpeas, coriander, ginger, turmeric, salt, pepper, and water or broth.",
                "Add firm vegetables such as carrots, turnips, and cabbage first, then add pumpkin and zucchini later so every vegetable finishes tender.",
                "Steam the couscous, rub it with water and oil to separate the grains, and repeat steaming until light and fluffy.",
                "Mound the couscous on a large platter and arrange the vegetables and chickpeas over the top.",
                "Ladle broth over the couscous and serve extra broth on the side."
            ]),
        new(
            "Tuscan Panzanella with Burrata",
            "A bright bread salad inspired by Tuscan panzanella, made with ripe tomatoes, cucumber, red onion, basil, olive oil, and vinegar. Burrata adds a modern creamy finish while the day-old bread soaks up the tomato juices.",
            "Lunch",
            "italian",
            "tuscany",
            "/images/recipes/italian-panzanella-burrata.webp",
            DifficultyLevel.Easy,
            30,
            "Panzanella con burrata",
            "Panzanella is a rustic Tuscan bread salad built around stale bread and summer produce; burrata is a contemporary addition rather than part of the older version.",
            false,
            "Summer lunch",
            [
                new("Day-old rustic bread", "300 g, torn"),
                new("Ripe tomatoes", "600 g, chopped"),
                new("Burrata", "250 g"),
                new("Cucumber", "1, sliced"),
                new("Red onion", "1/2, thinly sliced"),
                new("Fresh basil", "1 handful"),
                new("Extra-virgin olive oil", "5 tbsp"),
                new("Red wine vinegar", "2 tbsp"),
                new("Salt", "to taste"),
                new("Black pepper", "to taste")
            ],
            [
                "Toast or dry the torn bread lightly if it is not already stale.",
                "Toss tomatoes, cucumber, red onion, salt, vinegar, and olive oil until the tomatoes release their juices.",
                "Fold in the bread and let it absorb the dressing for several minutes.",
                "Add basil, adjust seasoning, and place torn burrata over the salad.",
                "Finish with olive oil and black pepper before serving."
            ]),
        new(
            "Italian Minestrone Soup",
            "A hearty vegetable minestrone with beans, tomatoes, herbs, and small pasta or rice. It is simple, filling, and flexible, with vegetables simmered until the broth becomes rich and comforting.",
            "Dinner",
            "italian",
            "lombardy",
            "/images/recipes/italian-minestrone-soup.webp",
            DifficultyLevel.Easy,
            60,
            "Minestrone di verdure",
            "Minestrone varies across Italy; this northern-style version uses beans, vegetables, rosemary, and a modest amount of pasta or rice for a cold-weather meal.",
            true,
            "Cold-weather family meal",
            [
                new("Cannellini beans", "1 can, drained"),
                new("Carrot", "2, diced"),
                new("Celery", "2 stalks, diced"),
                new("Onion", "1, diced"),
                new("Zucchini", "1, diced"),
                new("Potato", "1, diced"),
                new("Tomatoes", "400 g, chopped"),
                new("Vegetable stock", "1.5 liters"),
                new("Small pasta or rice", "100 g"),
                new("Garlic", "2 cloves, minced"),
                new("Rosemary", "1 sprig"),
                new("Olive oil", "3 tbsp"),
                new("Salt", "to taste"),
                new("Black pepper", "to taste")
            ],
            [
                "Soften onion, carrot, celery, and garlic in olive oil with a pinch of salt.",
                "Add potato, tomatoes, beans, rosemary, and vegetable stock, then simmer until the vegetables begin to soften.",
                "Stir in zucchini and small pasta or rice and cook until tender.",
                "Remove the rosemary stem, adjust salt and pepper, and rest the soup briefly.",
                "Serve warm with olive oil and grated cheese if desired."
            ]),
        new(
            "Italian Fried Artichokes",
            "Crisp fried artichokes coated with flour, egg, breadcrumbs, Parmesan, and parsley. Lemon keeps the trimmed artichokes bright before frying and adds freshness at the table.",
            "Lunch",
            "italian",
            null,
            "/images/recipes/italian-fried-artichokes.webp",
            DifficultyLevel.Medium,
            45,
            "Carciofi fritti",
            "Fried artichokes are found in several Italian regional traditions. No specific matching seeded region is assigned here because the current reference data does not include Lazio or Rome.",
            true,
            "Appetizer or shared starter",
            [
                new("Fresh artichokes", "6"),
                new("Lemon", "1"),
                new("Flour", "1 cup"),
                new("Eggs", "2, beaten"),
                new("Breadcrumbs", "1.5 cups"),
                new("Parmesan", "1/2 cup, grated"),
                new("Salt", "to taste"),
                new("Black pepper", "to taste"),
                new("Frying oil", "for frying"),
                new("Fresh parsley", "2 tbsp, chopped")
            ],
            [
                "Trim the artichokes, remove tough leaves, cut into wedges, and hold them in lemon water.",
                "Mix breadcrumbs with Parmesan, parsley, salt, and black pepper.",
                "Dry the artichokes, then coat each piece in flour, beaten egg, and seasoned breadcrumbs.",
                "Fry in hot oil in batches until crisp and golden.",
                "Drain on paper towels and serve hot with lemon wedges."
            ]),
        new(
            "Italian Summer Spaghetti",
            "A quick spaghetti dish with ripe tomatoes, garlic, basil, olive oil, Parmesan, and a little chili. The sauce is barely cooked so it keeps the freshness of summer tomatoes.",
            "Dinner",
            "italian",
            "campania",
            "/images/recipes/italian-summer-spaghetti.webp",
            DifficultyLevel.Easy,
            25,
            "Spaghetti estivi al pomodoro",
            "This modern summer pasta draws from southern Italian tomato-and-basil cooking associated with Campania.",
            false,
            "Quick summer dinner",
            [
                new("Spaghetti", "400 g"),
                new("Ripe tomatoes", "600 g, chopped"),
                new("Garlic", "3 cloves, sliced"),
                new("Fresh basil", "1 handful"),
                new("Extra-virgin olive oil", "5 tbsp"),
                new("Salt", "to taste"),
                new("Black pepper", "to taste"),
                new("Parmesan", "1/2 cup, grated"),
                new("Chili flakes", "1/2 tsp")
            ],
            [
                "Cook spaghetti in well-salted boiling water until just al dente.",
                "Warm olive oil with garlic and chili flakes until fragrant but not browned.",
                "Add tomatoes and cook briefly until juicy while still fresh-tasting.",
                "Toss spaghetti with the tomato sauce, basil, and a splash of pasta water.",
                "Finish with Parmesan, black pepper, and more olive oil."
            ]),
        new(
            "French Pain Perdu with Berries",
            "A French-style pain perdu made from day-old brioche soaked in vanilla custard, browned in butter, and served with berries, powdered sugar, and maple syrup or honey.",
            "Breakfast",
            "french",
            null,
            "/images/recipes/french-toast-breakfast.avif",
            DifficultyLevel.Easy,
            25,
            "Pain perdu",
            "Pain perdu means lost bread, a practical French way to turn stale bread into a tender breakfast or brunch dish.",
            true,
            "Breakfast or brunch",
            [
                new("Day-old brioche or bread", "8 slices"),
                new("Eggs", "3"),
                new("Milk", "1 cup"),
                new("Vanilla", "1 tsp"),
                new("Cinnamon", "1/2 tsp"),
                new("Butter", "3 tbsp"),
                new("Fresh berries", "2 cups"),
                new("Powdered sugar", "for serving"),
                new("Maple syrup or honey", "for serving"),
                new("Salt", "1 pinch")
            ],
            [
                "Whisk eggs, milk, vanilla, cinnamon, and salt into a smooth custard.",
                "Soak the bread slices until they absorb the custard without falling apart.",
                "Melt butter in a skillet and cook the soaked bread until golden on both sides.",
                "Keep cooked slices warm while finishing the remaining bread.",
                "Serve with berries, powdered sugar, and maple syrup or honey."
            ]),
        new(
            "New York Bagel with Lox",
            "A classic bagel breakfast layered with cream cheese, smoked salmon, red onion, capers, tomato, cucumber, dill, lemon, and black pepper.",
            "Breakfast",
            "international",
            null,
            "/images/recipes/new-york-bagel-lox.avif",
            DifficultyLevel.Easy,
            15,
            "Bagel with lox and cream cheese",
            "This deli-style breakfast is strongly associated with New York Jewish food culture and is classified as International in the current cuisine model.",
            true,
            "Breakfast or brunch",
            [
                new("Bagels", "4, split"),
                new("Smoked salmon", "250 g"),
                new("Cream cheese", "200 g"),
                new("Red onion", "1/2, thinly sliced"),
                new("Capers", "3 tbsp"),
                new("Tomato", "1, sliced"),
                new("Cucumber", "1/2, sliced"),
                new("Fresh dill", "2 tbsp"),
                new("Lemon", "1, wedges"),
                new("Black pepper", "to taste")
            ],
            [
                "Toast the bagels until warm and lightly crisp.",
                "Spread cream cheese generously over each cut side.",
                "Layer smoked salmon, tomato, cucumber, red onion, and capers over the bagels.",
                "Finish with dill, black pepper, and a squeeze of lemon.",
                "Serve open-faced or closed as a breakfast sandwich."
            ]),
        new(
            "American Buttermilk Pancakes",
            "Fluffy buttermilk pancakes made with a lightly sweet batter and served with maple syrup and fresh fruit. The batter rests briefly so the pancakes cook tender inside with golden edges.",
            "Breakfast",
            "international",
            null,
            "/images/recipes/american-buttermilk-pancakes.avif",
            DifficultyLevel.Easy,
            30,
            "Buttermilk pancakes",
            "Buttermilk pancakes are a familiar American weekend breakfast and are classified as International in the current cuisine model.",
            true,
            "Weekend breakfast",
            [
                new("Flour", "2 cups"),
                new("Buttermilk", "2 cups"),
                new("Eggs", "2"),
                new("Butter", "4 tbsp, melted"),
                new("Sugar", "2 tbsp"),
                new("Baking powder", "2 tsp"),
                new("Baking soda", "1/2 tsp"),
                new("Salt", "1/2 tsp"),
                new("Vanilla", "1 tsp"),
                new("Maple syrup", "for serving"),
                new("Fresh fruit", "for serving")
            ],
            [
                "Whisk flour, sugar, baking powder, baking soda, and salt in a bowl.",
                "Mix buttermilk, eggs, vanilla, and melted butter in a second bowl.",
                "Fold wet ingredients into dry ingredients just until combined and rest briefly.",
                "Cook ladles of batter on a buttered griddle until bubbles form, then flip and brown the second side.",
                "Serve warm with maple syrup and fresh fruit."
            ]),
        new(
            "Memphis-Style Fried Chicken",
            "A crisp, peppery fried chicken inspired by Memphis and Southern cooking. The chicken is marinated in buttermilk, dredged in seasoned flour and cornstarch, fried in batches, and rested before serving.",
            "Dinner",
            "international",
            null,
            "/images/recipes/memphis-fried-chicken.webp",
            DifficultyLevel.Medium,
            90,
            "Southern fried chicken",
            "Memphis-style fried chicken belongs to Southern American cooking traditions and is classified as International in the current cuisine model.",
            true,
            "Shared family meal",
            [
                new("Chicken pieces", "1.5 kg"),
                new("Buttermilk", "2 cups"),
                new("Flour", "2 cups"),
                new("Cornstarch", "1/2 cup"),
                new("Paprika", "2 tsp"),
                new("Garlic powder", "1 tsp"),
                new("Onion powder", "1 tsp"),
                new("Cayenne pepper", "1/2 tsp"),
                new("Salt", "2 tsp"),
                new("Black pepper", "1 tsp"),
                new("Frying oil", "for frying")
            ],
            [
                "Marinate the chicken pieces in buttermilk with some salt and pepper for at least 30 minutes.",
                "Whisk flour, cornstarch, paprika, garlic powder, onion powder, cayenne, salt, and black pepper.",
                "Lift chicken from the marinade and coat each piece thoroughly in the seasoned flour mixture.",
                "Fry in hot oil in batches, keeping the oil temperature steady and avoiding crowding.",
                "Check that the chicken is cooked through, then rest on a rack before serving."
            ])
    ];

    private sealed record RecipeSeedDefinition(
        string Title,
        string Description,
        string CategoryName,
        string CuisineSlug,
        string? RegionSlug,
        string ImageUrl,
        DifficultyLevel Difficulty,
        int PreparationTimeMinutes,
        string? TraditionalName,
        string OriginDescription,
        bool IsTraditional,
        string? ServingOccasion,
        IReadOnlyCollection<IngredientSeedDefinition> Ingredients,
        IReadOnlyCollection<string> Steps);

    private sealed record IngredientSeedDefinition(string Name, string Quantity);
}
