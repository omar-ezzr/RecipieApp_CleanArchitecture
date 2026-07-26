namespace Infrastructure.Seed;

public sealed class RecipeSeedOptions
{
    public const string SectionName = "RecipeSeed";

    public bool ResetRecipes { get; set; }
    public bool SeedRealRecipes { get; set; } = true;
}
