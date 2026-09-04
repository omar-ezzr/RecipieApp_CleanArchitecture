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
            .StrongPassword();

        RuleFor(dto => dto.Role)
            .NotEmpty()
            .Must(AppRoles.IsSupported)
            .WithMessage("Role must be User, Operator, or Admin.");
    }
}
