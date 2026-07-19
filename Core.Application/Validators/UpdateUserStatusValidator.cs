using Core.Application.DTO.Users;
using FluentValidation;

namespace Core.Application.Validators;

public sealed class UpdateUserStatusValidator : AbstractValidator<UpdateUserStatusDto>
{
    public UpdateUserStatusValidator()
    {
        RuleFor(dto => dto).NotNull();
    }
}
