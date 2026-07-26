namespace Core.Application.DTO
{
    public class RecipeQueryParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }
        public string? Difficulty { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? CuisineId { get; set; }
        public Guid? RegionId { get; set; }
        public bool? IsTraditional { get; set; }

        public string? SortBy { get; set; } = "CreatedAt";
    }
}
