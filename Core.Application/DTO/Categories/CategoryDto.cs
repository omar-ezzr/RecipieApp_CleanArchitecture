namespace Core.Application.DTO.Categories;

public sealed class CategoryDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
}
