namespace Core.Application.Common;
public sealed class RecipeMediaValidationException(string code, string message) : Exception(message) { public string Code { get; } = code; }
