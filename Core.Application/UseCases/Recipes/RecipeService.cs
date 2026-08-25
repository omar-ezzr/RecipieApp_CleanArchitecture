using Core.Application.Common;
using Core.Application.DTO;
using Core.Application.DTO.Recipe;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Services;
using Core.Application.DTO.Users;
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
       private RecipieDto MapToDto(Recipie r, RecipeLikeStatsDto? likeStats = null)
{
    return new RecipieDto
    {
        Id = r.Id,
        Title = r.Title,
        Description = r.Description,
        PreparationTimeMinutes = r.PreparationTimeMinutes,
        CategoryId = r.CategoryId,
        CuisineId = r.CuisineId,
        CuisineName = r.Cuisine != null ? r.Cuisine.Name : "Unknown",
        CuisineSlug = r.Cuisine != null ? r.Cuisine.Slug : "unknown",
        RegionId = r.RegionId,
        RegionName = r.Region?.Name,
        RegionSlug = r.Region?.Slug,
        ImageUrl = r.ImageUrl,
        Difficulty = r.Difficulty,
        Category = r.Category != null ? r.Category.Name : "Unknown",
        TraditionalName = r.TraditionalName,
        OriginDescription = r.OriginDescription,
        IsTraditional = r.IsTraditional,
        ServingOccasion = r.ServingOccasion,
        LikeCount = likeStats?.LikeCount ?? 0,
        IsLikedByCurrentUser = likeStats?.IsLikedByCurrentUser ?? false,
        Author = new AuthorDto
        {
            Id = r.UserId,
            DisplayName = r.User != null ? r.User.DisplayName : "Unknown author",
            AvatarUrl = r.User?.AvatarUrl
        },

        Ingredients = r.Ingredients != null
            ? r.Ingredients
                .Select(i => new CreateIngredientDto
                {
                    Name = i.Name,
                    Quantity = i.Quantity
                })
                .ToList()
            : [],

        Steps = r.Steps != null
            ? r.Steps
                .OrderBy(s => s.StepNumber)
                .Select(s => new CreateRecipeStepDto
                {
                    StepNumber = s.StepNumber,
                    Instruction = s.Instruction
                })
                .ToList()
            : []
    };
}

        // 🔹 GET BY ID
        public async Task<RecipieDto?> GetByIdAsync(Guid id, Guid? currentUserId = null, CancellationToken cancellationToken = default)
        {
            var recipe = await _repository.GetByIdAsync(id, cancellationToken);
            if (recipe is null) return null;

            var stats = await _repository.GetLikeStatsAsync([recipe.Id], currentUserId, cancellationToken);

            return MapToDto(recipe, stats.GetValueOrDefault(recipe.Id));
        }

        // 🔹 CREATE
        public async Task<ServiceResult<RecipieDto>> CreateAsync(CreateRecipeDto dto, Guid currentUserId, CancellationToken cancellationToken = default)
        {
            if (!IsDefinedDifficulty(dto.Difficulty))
            {
                return ServiceResult<RecipieDto>.Failure("Difficulty must be Easy, Medium, or Hard", ServiceErrorType.Validation);
            }

            if (!await _repository.CategoryExistsAsync(dto.CategoryId, cancellationToken))
            {
                return ServiceResult<RecipieDto>.Failure("Category not found", ServiceErrorType.Validation);
            }

            var cultureValidation = await ValidateCultureAsync(dto.CuisineId, dto.RegionId, cancellationToken);
            if (cultureValidation is not null)
            {
                return ServiceResult<RecipieDto>.Failure(cultureValidation, ServiceErrorType.Validation);
            }

            var recipe = new Recipie
            {
                Id = Guid.NewGuid(),
                Title = Normalize(dto.Title),
                Description = Normalize(dto.Description),
                PreparationTimeMinutes = dto.PreparationTimeMinutes,
                CategoryId = dto.CategoryId,
                CuisineId = dto.CuisineId,
                RegionId = dto.RegionId,
                UserId = currentUserId,
                ImageUrl = NormalizeOptional(dto.ImageUrl),
                Difficulty = dto.Difficulty,
                TraditionalName = NormalizeOptional(dto.TraditionalName),
                OriginDescription = NormalizeOptional(dto.OriginDescription),
                IsTraditional = dto.IsTraditional,
                ServingOccasion = NormalizeOptional(dto.ServingOccasion),
                Ingredients = dto.Ingredients
                    .Select(ingredient => new Ingredient
                    {
                        Id = Guid.NewGuid(),
                        Name = Normalize(ingredient.Name),
                        Quantity = ingredient.Quantity?.Trim() ?? string.Empty,
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToList(),
                Steps = dto.Steps
                    .OrderBy(step => step.StepNumber)
                    .Select(step => new RecipieStep
                    {
                        Id = Guid.NewGuid(),
                        StepNumber = step.StepNumber,
                        Instruction = Normalize(step.Instruction),
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToList()
            };

            await _repository.AddAsync(recipe, cancellationToken);

            var created = await _repository.GetByIdAsync(recipe.Id, cancellationToken);

            return ServiceResult<RecipieDto>.Success(MapToDto(created ?? recipe));
        }

        // 🔹 UPDATE
        public async Task<ServiceResult<RecipieDto>> UpdateAsync(
            Guid id,
            CreateRecipeDto dto,
            Guid currentUserId,
            bool isAdmin,
            CancellationToken cancellationToken = default)
        {
            if (!IsDefinedDifficulty(dto.Difficulty))
            {
                return ServiceResult<RecipieDto>.Failure("Difficulty must be Easy, Medium, or Hard", ServiceErrorType.Validation);
            }

            var recipe = await _repository.GetByIdAsync(id, cancellationToken);

            if (recipe == null)
                return ServiceResult<RecipieDto>.Failure("Recipe not found", ServiceErrorType.NotFound);

            if (!isAdmin && recipe.UserId != currentUserId)
                return ServiceResult<RecipieDto>.Failure("You can only update your own recipe.", ServiceErrorType.Forbidden);

            if (!await _repository.CategoryExistsAsync(dto.CategoryId, cancellationToken))
            {
                return ServiceResult<RecipieDto>.Failure("Category not found", ServiceErrorType.Validation);
            }

            var cultureValidation = await ValidateCultureAsync(dto.CuisineId, dto.RegionId, cancellationToken);
            if (cultureValidation is not null)
            {
                return ServiceResult<RecipieDto>.Failure(cultureValidation, ServiceErrorType.Validation);
            }

            recipe.Title = Normalize(dto.Title);
            recipe.Description = Normalize(dto.Description);
            recipe.PreparationTimeMinutes = dto.PreparationTimeMinutes;
            recipe.CategoryId = dto.CategoryId;
            recipe.CuisineId = dto.CuisineId;
            recipe.RegionId = dto.RegionId;
            recipe.ImageUrl = NormalizeOptional(dto.ImageUrl);
            recipe.Difficulty = dto.Difficulty;
            recipe.TraditionalName = NormalizeOptional(dto.TraditionalName);
            recipe.OriginDescription = NormalizeOptional(dto.OriginDescription);
            recipe.IsTraditional = dto.IsTraditional;
            recipe.ServingOccasion = NormalizeOptional(dto.ServingOccasion);
            recipe.Ingredients.Clear();
            foreach (var ingredient in dto.Ingredients)
            {
                recipe.Ingredients.Add(new Ingredient
                {
                    RecipeId = recipe.Id,
                    Name = Normalize(ingredient.Name),
                    Quantity = ingredient.Quantity?.Trim() ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                });
            }

            recipe.Steps.Clear();
            foreach (var step in dto.Steps.OrderBy(step => step.StepNumber))
            {
                recipe.Steps.Add(new RecipieStep
                {
                    RecipeId = recipe.Id,
                    StepNumber = step.StepNumber,
                    Instruction = Normalize(step.Instruction),
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _repository.UpdateAsync(recipe, cancellationToken);

            return ServiceResult<RecipieDto>.Success(MapToDto(recipe));
        }

        // 🔹 DELETE
        public async Task<ServiceResult> DeleteAsync(
            Guid id,
            Guid currentUserId,
            bool isAdmin,
            CancellationToken cancellationToken = default)
        {
            var recipe = await _repository.GetByIdAsync(id, cancellationToken);

            if (recipe == null)
                return ServiceResult.Failure("Recipe not found", ServiceErrorType.NotFound);

            if (!isAdmin && recipe.UserId != currentUserId)
                return ServiceResult.Failure("You can only delete your own recipe.", ServiceErrorType.Forbidden);

            await _repository.DeleteAsync(recipe, cancellationToken);

            return ServiceResult.Success();
        }

        // 🔹 PAGINATION + FILTERING
        public async Task<PagedResult<RecipieDto>> GetPagedAsync(
            RecipeQueryParams parameters,
            Guid? currentUserId = null,
            CancellationToken cancellationToken = default)
        {
            var paged = await _repository.GetPagedAsync(parameters, cancellationToken);

            var stats = await _repository.GetLikeStatsAsync(
                paged.Items.Select(recipe => recipe.Id).ToList(),
                currentUserId,
                cancellationToken);

            var result = paged.Items
                .Select(recipe => MapToDto(recipe, stats.GetValueOrDefault(recipe.Id)))
                .ToList();

            return new PagedResult<RecipieDto>
            {
                Items = result,
                Total = paged.Total,
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalPages = paged.TotalPages
            };
        }

        public async Task<PagedResult<RecipieDto>> GetMineAsync(
            RecipeQueryParams parameters,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            parameters.UserId = currentUserId;

            return await GetPagedAsync(parameters, currentUserId, cancellationToken);
        }

        private static bool IsDefinedDifficulty(DifficultyLevel difficulty)
        {
            return Enum.IsDefined(typeof(DifficultyLevel), difficulty);
        }

        private static string Normalize(string value) => value.Trim();

        private static string? NormalizeOptional(string? value)
        {
            var normalized = value?.Trim();

            return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
        }

        private async Task<string?> ValidateCultureAsync(Guid cuisineId, Guid? regionId, CancellationToken cancellationToken)
        {
            if (cuisineId == Guid.Empty)
            {
                return "Cuisine is required";
            }

            if (!await _repository.CuisineExistsAsync(cuisineId, cancellationToken))
            {
                return "Cuisine not found or inactive";
            }

            if (!regionId.HasValue)
            {
                return null;
            }

            var region = await _repository.GetActiveRegionAsync(regionId.Value, cancellationToken);
            if (region is null)
            {
                return "Region not found or inactive";
            }

            return region.CuisineId == cuisineId
                ? null
                : "The region does not belong to the selected cuisine.";
        }
    }
}
