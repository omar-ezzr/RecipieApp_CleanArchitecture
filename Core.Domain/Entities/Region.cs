using Core.Domain.Common;

namespace Core.Domain.Entities;

public class Region : BaseEntity
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public Guid CuisineId { get; set; }
    public required Cuisine Cuisine { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Recipie> Recipes { get; set; } = [];
}
