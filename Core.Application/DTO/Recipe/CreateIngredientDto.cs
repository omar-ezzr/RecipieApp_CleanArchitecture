namespace Core.Application.DTO.Recipe;

public sealed class CreateIngredientDto
{
    public required string Name { get; set; }
    public string Quantity { get; set; } = string.Empty;
}
