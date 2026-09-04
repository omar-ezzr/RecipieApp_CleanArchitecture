namespace API.Options;

public sealed class RecipeImageOptions
{
    public const string SectionName = "RecipeImages";
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
    public string[] AllowedContentTypes { get; set; } = ["image/jpeg", "image/png", "image/webp"];
}
