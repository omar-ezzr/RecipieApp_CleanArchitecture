using Core.Application.DTO.Users;
using Core.Domain.Constants;
using FluentValidation;

namespace Core.Application.Validators;

public sealed class CreateUserAccountValidator : AbstractValidator<CreateUserAccountDto>
{
    public CreateUserAccountValidator()
    {
        RuleFor(dto => dto.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(dto => dto.DisplayName)
            .MaximumLength(100)
            .When(dto => !string.IsNullOrWhiteSpace(dto.DisplayName));

        RuleFor(dto => dto.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Must(password => password.Any(char.IsUpper))
            .WithMessage("Password must contain an uppercase letter.")
            .Must(password => password.Any(char.IsLower))
            .WithMessage("Password must contain a lowercase letter.")
            .Must(password => password.Any(char.IsDigit))
            .WithMessage("Password must contain a number.");

        RuleFor(dto => dto.Role)
            .NotEmpty()
            .Must(AppRoles.IsSupported)
            .WithMessage("Role must be User, Operator, or Admin.");
    }
}
