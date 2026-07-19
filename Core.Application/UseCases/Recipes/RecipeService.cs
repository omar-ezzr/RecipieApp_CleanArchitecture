using Core.Application.Common;
using Core.Application.DTO;
using Core.Application.DTO.Recipe;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Services;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Application.UseCases.Recipes
{
    public class RecipeService : IRecipeService
    {
        private readonly IRecipeRepository _repository;

        public RecipeService(IRecipeRepository repository)
        {
            _repository = repository;
        }

        // 🔹 CENTRALIZED MAPPER (critical)
       private RecipieDto MapToDto(Recipie r)
{
    return new RecipieDto
    {
        Id = r.Id,
        Title = r.Title,
        Description = r.Description,
        PreparationTimeMinutes = r.PreparationTimeMinutes,
        CategoryId = r.CategoryId,
        ImageUrl = r.ImageUrl,
        Difficulty = r.Difficulty.ToString(),
        Category = r.Category != null ? r.Category.Name : "Unknown",

        Ingredients = r.Ingredients != null
            ? r.Ingredients
                .Select(i => i.Name)
                .ToList()
            : new List<string>(),

        Steps = r.Steps != null
            ? r.Steps
                .OrderBy(s => s.StepNumber)
                .Select(s => s.Instruction)
                .ToList()
            : new List<string>()
    };
}

        // 🔹 GET BY ID
        public async Task<RecipieDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var recipe = await _repository.GetByIdAsync(id, cancellationToken);
            if (recipe is null) return null;

            return MapToDto(recipe);
        }

        // 🔹 CREATE
        public async Task<Result> CreateAsync(CreateRecipeDto dto, CancellationToken cancellationToken = default)
        {
            if (!TryParseDifficulty(dto.Difficulty, out var difficulty))
            {
                return Result.Failure("Difficulty must be Easy, Medium, or Hard");
            }

            var recipe = new Recipie
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                PreparationTimeMinutes = dto.PreparationTimeMinutes,
                CategoryId = dto.CategoryId,
                ImageUrl = dto.ImageUrl,
                Difficulty = difficulty
            };

            await _repository.AddAsync(recipe, cancellationToken);
            return Result.Success();
        }

        // 🔹 UPDATE
        public async Task<Result> UpdateAsync(Guid id, CreateRecipeDto dto, CancellationToken cancellationToken = default)
        {
            if (!TryParseDifficulty(dto.Difficulty, out var difficulty))
            {
                return Result.Failure("Difficulty must be Easy, Medium, or Hard");
            }

            var recipe = await _repository.GetByIdAsync(id, cancellationToken);

            if (recipe == null)
                return Result.Failure("Recipe not found");

            recipe.Title = dto.Title;
            recipe.Description = dto.Description;
            recipe.PreparationTimeMinutes = dto.PreparationTimeMinutes;
            recipe.CategoryId = dto.CategoryId;
            recipe.ImageUrl = dto.ImageUrl;
            recipe.Difficulty = difficulty;

            await _repository.UpdateAsync(recipe, cancellationToken);

            return Result.Success();
        }

        // 🔹 DELETE
        public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var recipe = await _repository.GetByIdAsync(id, cancellationToken);

            if (recipe == null)
                return Result.Failure("Recipe not found");

            await _repository.DeleteAsync(recipe, cancellationToken);

            return Result.Success();
        }

        // 🔹 PAGINATION + FILTERING
        public async Task<PagedResult<RecipieDto>> GetPagedAsync(
            RecipeQueryParams parameters,
            CancellationToken cancellationToken = default)
        {
            var paged = await _repository.GetPagedAsync(parameters, cancellationToken);

            var result = paged.Items.Select(MapToDto).ToList();

            return new PagedResult<RecipieDto>
            {
                Items = result,
                Total = paged.Total,
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalPages = paged.TotalPages
            };
        }

        private static bool TryParseDifficulty(string? value, out DifficultyLevel difficulty)
        {
            return Enum.TryParse(value, true, out difficulty)
                && Enum.IsDefined(typeof(DifficultyLevel), difficulty);
        }
    }
}
