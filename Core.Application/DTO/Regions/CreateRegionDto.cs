namespace Core.Application.DTO.Regions;

public sealed class CreateRegionDto
{
    public required string Name { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public Guid CuisineId { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
