using Core.Application.DTO.Users;
using Core.Domain.Constants;
using FluentValidation;

namespace Core.Application.Validators;

public sealed class UpdateUserRoleValidator : AbstractValidator<UpdateUserRoleDto>
{
    public UpdateUserRoleValidator()
    {
        RuleFor(dto => dto.Role)
            .NotEmpty()
            .Must(AppRoles.IsSupported)
            .WithMessage("Role must be User, Operator, or Admin.");
    }
}
