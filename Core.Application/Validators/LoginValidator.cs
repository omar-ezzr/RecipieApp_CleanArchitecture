using Core.Application.DTO.Auth;
using FluentValidation;

namespace Core.Application.Validators;

public sealed class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(dto => dto.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(dto => dto.Password).NotEmpty().MaximumLength(128);
    }
}
