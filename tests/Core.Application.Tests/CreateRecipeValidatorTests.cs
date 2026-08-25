using Core.Application.DTO.Recipe;
using Core.Application.Validators;
using Core.Domain.Enums;

namespace Core.Application.Tests;

public class CreateRecipeValidatorTests
{
    private readonly CreateRecipeValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://example.com/recipe.jpg")]
    [InlineData("http://example.com/recipe.jpg")]
    [InlineData("/images/recipes/memphis-fried-chicken.webp")]
    public void ImageUrl_accepts_empty_http_https_and_application_image_paths(string? imageUrl)
    {
        var result = _validator.Validate(NewRecipe(imageUrl));

        Assert.DoesNotContain(result.Errors, error => error.PropertyName == nameof(CreateRecipeDto.ImageUrl));
    }

    [Theory]
    [InlineData("images/recipes/memphis-fried-chicken.webp")]
    [InlineData("/uploads/recipe.webp")]
    [InlineData("ftp://example.com/recipe.jpg")]
    [InlineData("/images/../secret.txt")]
    public void ImageUrl_rejects_malformed_or_unsupported_paths(string imageUrl)
    {
        var result = _validator.Validate(NewRecipe(imageUrl));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateRecipeDto.ImageUrl));
    }

    [Theory]
    [InlineData("")]
    [InlineData("2 tbsp")]
    public void Ingredient_quantity_allows_blank_or_text_values(string quantity)
    {
        var recipe = NewRecipe(null);
        recipe.Ingredients = [new CreateIngredientDto { Name = "Salt", Quantity = quantity }];

        var result = _validator.Validate(recipe);

        Assert.DoesNotContain(result.Errors, error => error.PropertyName.Contains(nameof(CreateIngredientDto.Quantity)));
    }


    [Fact]
    public void Ingredient_quantity_defaults_to_blank_when_omitted()
    {
        var ingredient = new CreateIngredientDto { Name = "Salt" };
        var recipe = NewRecipe(null);
        recipe.Ingredients = [ingredient];

        var result = _validator.Validate(recipe);

        Assert.Equal(string.Empty, ingredient.Quantity);
        Assert.DoesNotContain(result.Errors, error => error.PropertyName.Contains(nameof(CreateIngredientDto.Quantity)));
    }

    [Fact]
    public void Ingredient_quantity_rejects_values_over_maximum_length()
    {
        var recipe = NewRecipe(null);
        recipe.Ingredients = [new CreateIngredientDto { Name = "Salt", Quantity = new string('x', 101) }];

        var result = _validator.Validate(recipe);

        Assert.Contains(result.Errors, error => error.PropertyName.Contains(nameof(CreateIngredientDto.Quantity)));
    }

    [Fact]
    public void Missing_category_fails_with_readable_message()
    {
        var recipe = NewRecipe(null);
        recipe.CategoryId = Guid.Empty;

        var result = _validator.Validate(recipe);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateRecipeDto.CategoryId) && error.ErrorMessage == "Please select a category.");
    }

    [Fact]
    public void Missing_cuisine_fails_with_readable_message()
    {
        var recipe = NewRecipe(null);
        recipe.CuisineId = Guid.Empty;

        var result = _validator.Validate(recipe);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateRecipeDto.CuisineId) && error.ErrorMessage == "Please select a cuisine.");
    }

    [Fact]
    public void Empty_step_instruction_still_fails()
    {
        var recipe = NewRecipe(null);
        recipe.Steps = [new CreateRecipeStepDto { StepNumber = 1, Instruction = "" }];

        var result = _validator.Validate(recipe);

        Assert.Contains(result.Errors, error => error.PropertyName.Contains(nameof(CreateRecipeStepDto.Instruction)) && error.ErrorMessage == "Step instruction is required");
    }


    private static CreateRecipeDto NewRecipe(string? imageUrl)
    {
        return new CreateRecipeDto
        {
            Title = "Soup",
            Description = "Warm",
            PreparationTimeMinutes = 20,
            CategoryId = Guid.NewGuid(),
            CuisineId = Guid.NewGuid(),
            Difficulty = DifficultyLevel.Easy,
            ImageUrl = imageUrl,
            Ingredients = [new CreateIngredientDto { Name = "Salt", Quantity = "1 tsp" }],
            Steps = [new CreateRecipeStepDto { StepNumber = 1, Instruction = "Cook" }]
        };
    }
}
