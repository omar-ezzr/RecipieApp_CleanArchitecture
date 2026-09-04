namespace Core.Application.Common;

public sealed class RecipeImageValidationException : Exception
{
    public RecipeImageValidationException(string code, string message) : base(message) => Code = code;
    public string Code { get; }
}
