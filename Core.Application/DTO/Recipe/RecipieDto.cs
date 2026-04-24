public class RecipieDto
{
    public Guid Id { get; set; }

    public required string Title { get; set; }
    public required string Description { get; set; }

    public int PreparationTimeMinutes { get; set; }

    public required string Category { get; set; }

    public string? ImageUrl { get; set; }

    public string? Difficulty { get; set; }

    public List<string> Ingredients { get; set; } = new();

    public List<string> Steps { get; set; } = new();
}