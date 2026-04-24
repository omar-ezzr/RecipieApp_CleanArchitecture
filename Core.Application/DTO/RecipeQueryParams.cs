using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.DTO
{
    public class RecipeQueryParams
    {
          public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }
    public string? Difficulty { get; set; }
    public int? CategoryId { get; set; }

    public string? SortBy { get; set; } = "CreatedAt";
    }
}