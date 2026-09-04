using FluentValidation;

namespace Core.Application.Validators;

public static class PasswordRuleExtensions
{
    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128)
            .Must(password => password?.Any(char.IsUpper) == true).WithMessage("Password must contain an uppercase letter.")
            .Must(password => password?.Any(char.IsLower) == true).WithMessage("Password must contain a lowercase letter.")
            .Must(password => password?.Any(char.IsDigit) == true).WithMessage("Password must contain a number.");
    }
}
