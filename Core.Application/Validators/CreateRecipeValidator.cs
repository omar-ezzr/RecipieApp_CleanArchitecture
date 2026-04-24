using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Core.Application.DTO.Recipe;

namespace Core.Application.Validators
{
  public class CreateRecipeValidator : AbstractValidator<CreateRecipeDto>
{
    public CreateRecipeValidator()
    {

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required");

      RuleFor(x => x.Description)
    .NotEmpty().WithMessage("Description is required TEST");
    
        RuleFor(x => x.PreparationTimeMinutes)
            .GreaterThan(0).WithMessage("Preparation time must be > 0");

        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty)
            .WithMessage("Category is required");
    }
}
}