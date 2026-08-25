using FluentValidation;
using Core.Application.DTO.Recipe;
using Core.Domain.Enums;

namespace Core.Application.Validators
{
  public class CreateRecipeValidator : AbstractValidator<CreateRecipeDto>
{
    public CreateRecipeValidator()
    {

        RuleFor(x => x.Title)
            .Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must be 200 characters or fewer");

        RuleFor(x => x.Description)
            .Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("Description is required")
            .MaximumLength(5000).WithMessage("Description must be 5,000 characters or fewer");

        RuleFor(x => x.Difficulty)
            .Must(value => Enum.IsDefined(typeof(DifficultyLevel), value))
            .WithMessage("Difficulty must be Easy, Medium, or Hard");

        RuleFor(x => x.PreparationTimeMinutes)
            .GreaterThan(0).WithMessage("Preparation time must be > 0")
            .LessThanOrEqualTo(7 * 24 * 60).WithMessage("Preparation time is too large");

        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty)
            .WithMessage("Please select a category.");

        RuleFor(x => x.CuisineId)
            .NotEqual(Guid.Empty)
            .WithMessage("Please select a cuisine.");

        RuleFor(x => x.TraditionalName)
            .MaximumLength(200)
            .WithMessage("Traditional name must be 200 characters or fewer");

        RuleFor(x => x.OriginDescription)
            .MaximumLength(2000)
            .WithMessage("Origin description must be 2,000 characters or fewer");

        RuleFor(x => x.ServingOccasion)
            .MaximumLength(200)
            .WithMessage("Serving occasion must be 200 characters or fewer");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(2048)
            .Must(IsValidImageUrl)
            .WithMessage("Image URL must be an absolute HTTP or HTTPS URL or an application image path beginning with /images/");

        RuleFor(x => x.Ingredients)
            .NotEmpty().WithMessage("At least one ingredient is required")
            .Must(items => items.Count <= 100).WithMessage("A recipe can have at most 100 ingredients");

        RuleForEach(x => x.Ingredients).ChildRules(ingredient =>
        {
            ingredient.RuleFor(x => x.Name)
                .Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("Ingredient name is required")
                .MaximumLength(150).WithMessage("Ingredient name must be 150 characters or fewer");

            ingredient.RuleFor(x => x.Quantity)
                .MaximumLength(100).WithMessage("Ingredient quantity must be 100 characters or fewer");
        });

        RuleFor(x => x.Steps)
            .NotEmpty().WithMessage("At least one preparation step is required")
            .Must(items => items.Count <= 100).WithMessage("A recipe can have at most 100 steps")
            .Must(HaveUniqueStepNumbers).WithMessage("Step numbers must be unique")
            .Must(HaveSequentialStepNumbers).WithMessage("Step numbers must be sequential starting from 1");

        RuleForEach(x => x.Steps).ChildRules(step =>
        {
            step.RuleFor(x => x.StepNumber)
                .GreaterThan(0).WithMessage("Step number must be greater than zero");

            step.RuleFor(x => x.Instruction)
                .Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("Step instruction is required")
                .MaximumLength(1000).WithMessage("Step instruction must be 1,000 characters or fewer");
        });
    }

    private static bool IsValidImageUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var trimmed = value.Trim();

        if (trimmed.StartsWith("/images/", StringComparison.Ordinal))
        {
            return !trimmed.Contains("..", StringComparison.Ordinal) && !trimmed.Contains('\\');
        }

        return Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static bool HaveUniqueStepNumbers(IReadOnlyCollection<CreateRecipeStepDto> steps)
    {
        return steps.Select(step => step.StepNumber).Distinct().Count() == steps.Count;
    }

    private static bool HaveSequentialStepNumbers(IReadOnlyCollection<CreateRecipeStepDto> steps)
    {
        return steps
            .OrderBy(step => step.StepNumber)
            .Select((step, index) => step.StepNumber == index + 1)
            .All(isSequential => isSequential);
    }
}
}
