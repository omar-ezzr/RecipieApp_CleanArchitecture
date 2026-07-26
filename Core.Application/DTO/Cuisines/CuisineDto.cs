namespace Core.Application.DTO.Cuisines;

public sealed class CuisineDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public required string CountryCode { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int RegionCount { get; set; }
    public int RecipeCount { get; set; }
}
