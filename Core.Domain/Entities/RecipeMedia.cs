using Core.Domain.Common;
using Core.Domain.Enums;

namespace Core.Domain.Entities;

public sealed class RecipeMedia : BaseEntity
{
    public Guid RecipeId { get; set; }
    public Recipie Recipe { get; set; } = default!;
    public string Url { get; set; } = default!;
    public RecipeMediaType MediaType { get; set; }
    public string ContentType { get; set; } = default!;
    public bool IsMain { get; set; }
    public int SortOrder { get; set; }
}
