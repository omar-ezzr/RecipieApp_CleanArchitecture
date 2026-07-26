namespace Core.Application.DTO.Recipe;

public sealed class CreateIngredientDto
{
    public required string Name { get; set; }
    public required string Quantity { get; set; }
}
