using Core.Application.DTO.Auth;
using FluentValidation;

namespace Core.Application.Validators;

public sealed class TokenRequestValidator : AbstractValidator<TokenRequestDto>
{
    public TokenRequestValidator()
    {
        RuleFor(dto => dto.RefreshToken).NotEmpty().MaximumLength(512);
    }
}
