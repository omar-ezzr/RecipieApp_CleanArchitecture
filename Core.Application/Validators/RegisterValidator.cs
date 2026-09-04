using Core.Application.DTO.Auth;
using FluentValidation;

namespace Core.Application.Validators;

public sealed class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(dto => dto.DisplayName).NotEmpty().MaximumLength(100);
        RuleFor(dto => dto.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(dto => dto.Password).StrongPassword();
    }
}
