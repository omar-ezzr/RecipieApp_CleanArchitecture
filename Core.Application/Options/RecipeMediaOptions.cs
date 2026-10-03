namespace Core.Application.Options;

public sealed class RecipeMediaOptions
{
    public const string SectionName = "RecipeMedia";

    public int MaxItems { get; set; } = 9;
    public long MaxImageFileSizeBytes { get; set; } = 5 * 1024 * 1024;
    public string StoragePath { get; set; } = string.Empty;
    public string PublicPath { get; set; } = "/images/recipes";
    public string[] AllowedImageContentTypes { get; set; } = ["image/jpeg", "image/png", "image/webp"];
}
