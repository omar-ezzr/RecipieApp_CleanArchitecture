using Core.Domain.Common;

namespace Core.Domain.Entities;

public class Cuisine : BaseEntity
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public required string CountryCode { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Region> Regions { get; set; } = [];
    public ICollection<Recipie> Recipes { get; set; } = [];
}
