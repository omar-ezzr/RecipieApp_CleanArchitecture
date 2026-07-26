namespace Core.Application.DTO.Cuisines;

public sealed class UpdateCuisineDto
{
    public required string Name { get; set; }
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public required string CountryCode { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
