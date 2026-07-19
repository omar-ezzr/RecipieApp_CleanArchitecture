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
            .NotEmpty().WithMessage("Title is required");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required");

        RuleFor(x => x.Difficulty)
            .NotEmpty().WithMessage("Difficulty is required")
            .Must(value => Enum.TryParse<DifficultyLevel>(value, true, out var difficulty)
                && Enum.IsDefined(typeof(DifficultyLevel), difficulty))
            .WithMessage("Difficulty must be Easy, Medium, or Hard");
    
        RuleFor(x => x.PreparationTimeMinutes)
            .GreaterThan(0).WithMessage("Preparation time must be > 0");

        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty)
            .WithMessage("Category is required");
    }
}
}
