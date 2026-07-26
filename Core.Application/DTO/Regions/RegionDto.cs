namespace Core.Application.DTO.Regions;

public sealed class RegionDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public Guid CuisineId { get; set; }
    public required string CuisineName { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int RecipeCount { get; set; }
}
