using Core.Application.Common;
using Core.Application.DTO;
using Core.Application.DTO.Recipe;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Services;
using Core.Domain.Entities;

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

        // 🔹 GET ALL (use carefully, not for large datasets)
        public async Task<IEnumerable<RecipieDto>> GetAllAsync()
        {
            var recipes = await _repository.GetAllAsync();
            return recipes.Select(MapToDto);
        }

        // 🔹 GET BY ID
        public async Task<RecipieDto?> GetByIdAsync(Guid id)
        {
            var recipe = await _repository.GetByIdAsync(id);
            if (recipe is null) return null;

            return MapToDto(recipe);
        }

        // 🔹 CREATE
        public async Task<Result> CreateAsync(CreateRecipeDto dto)
        {
            var recipe = new Recipie
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                PreparationTimeMinutes = dto.PreparationTimeMinutes,
                CategoryId = dto.CategoryId,
                ImageUrl = dto.ImageUrl,
                Difficulty = dto.Difficulty // make sure DTO includes this
            };

            await _repository.AddAsync(recipe);
            return Result.Success();
        }

        // 🔹 UPDATE
        public async Task<Result> UpdateAsync(Guid id, CreateRecipeDto dto)
        {
            var recipe = await _repository.GetByIdAsync(id);

            if (recipe == null)
                return Result.Failure("Recipe not found");

            recipe.Title = dto.Title;
            recipe.Description = dto.Description;
            recipe.PreparationTimeMinutes = dto.PreparationTimeMinutes;
            recipe.CategoryId = dto.CategoryId;
            recipe.ImageUrl = dto.ImageUrl;
            recipe.Difficulty = dto.Difficulty;

            await _repository.UpdateAsync(recipe);

            return Result.Success();
        }

        // 🔹 DELETE
        public async Task<Result> DeleteAsync(Guid id)
        {
            var recipe = await _repository.GetByIdAsync(id);

            if (recipe == null)
                return Result.Failure("Recipe not found");

            await _repository.DeleteAsync(recipe);

            return Result.Success();
        }

        // 🔹 PAGINATION + FILTERING
        public async Task<(List<RecipieDto>, int)> GetPagedAsync(RecipeQueryParams parameters)
        {
            var (recipes, total) = await _repository.GetPagedAsync(parameters);

            var result = recipes.Select(MapToDto).ToList();

            return (result, total);
        }
    }
}
