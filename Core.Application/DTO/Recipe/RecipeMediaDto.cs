using Core.Domain.Enums;

namespace Core.Application.DTO.Recipe;

public sealed class RecipeMediaDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = default!;
    public RecipeMediaType MediaType { get; set; }
    public string ContentType { get; set; } = default!;
    public bool IsMain { get; set; }
    public int SortOrder { get; set; }
}
