using Core.Application.Common;
using Core.Application.DTO;
using Core.Application.DTO.Recipe;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Services;
using Core.Application.Options;
using Core.Application.DTO.Users;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Application.UseCases.Recipes
{
    public class RecipeService : IRecipeService
    {
        private readonly IRecipeRepository _repository;
        private readonly IRecipeMediaStorage? _mediaStorage;
        private readonly RecipeMediaOptions _mediaOptions;

        public RecipeService(
            IRecipeRepository repository,
            IRecipeMediaStorage? mediaStorage = null,
            RecipeMediaOptions? mediaOptions = null)
        {
            _repository = repository;
            _mediaStorage = mediaStorage;
            _mediaOptions = mediaOptions ?? new RecipeMediaOptions();
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

        Steps = r.Steps != null ? r.Steps.OrderBy(s => s.StepNumber).Select(s => new CreateRecipeStepDto { StepNumber = s.StepNumber, Instruction = s.Instruction }).ToList() : [],
        Media = OrderedImages(r).Select(m => new RecipeMediaDto { Id=m.Id, Url=m.Url, MediaType=m.MediaType, ContentType=m.ContentType, IsMain=m.IsMain, SortOrder=m.SortOrder }).ToList()
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
                ImageUrl = null,
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

            var urls = recipe.Media.Select(m => m.Url).Append(recipe.ImageUrl).Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().ToList();
            await _repository.DeleteAsync(recipe, cancellationToken);
            foreach (var url in urls) await _mediaStorage!.DeleteAsync(url!, cancellationToken);
            return ServiceResult.Success();
        }

        public async Task<ServiceResult<RecipeMediaDto>> AddMediaAsync(Guid id, Stream content, string fileName, string contentType, long length, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
        {
            var recipe = await _repository.GetByIdAsync(id, cancellationToken);
            if (recipe is null) return ServiceResult<RecipeMediaDto>.Failure("Recipe not found", ServiceErrorType.NotFound);
            if (!isAdmin && recipe.UserId != currentUserId) return ServiceResult<RecipeMediaDto>.Failure("You can only update your own recipe.", ServiceErrorType.Forbidden);

            var photos = OrderedImages(recipe);
            if (photos.Count >= _mediaOptions.MaxItems) return ServiceResult<RecipeMediaDto>.Failure("A recipe can have at most 9 photos.", ServiceErrorType.Validation);

            string? url = null;
            try
            {
                url = await (_mediaStorage ?? throw new InvalidOperationException("Recipe media storage is not configured."))
                    .SaveAsync(new RecipeMediaUpload { Content = content, FileName = fileName, ContentType = contentType, Length = length }, cancellationToken);

                var media = new RecipeMedia
                {
                    Id = Guid.NewGuid(), RecipeId = id, Url = url, ContentType = contentType,
                    MediaType = RecipeMediaType.Image, IsMain = photos.Count == 0,
                    SortOrder = photos.Count, CreatedAt = DateTime.UtcNow
                };
                recipe.Media.Add(media);
                await _repository.AddMediaAsync(media, cancellationToken);
                ResolveCover(recipe);
                await _repository.UpdateAsync(recipe, cancellationToken);
                return ServiceResult<RecipeMediaDto>.Success(ToMediaDto(media));
            }
            catch (RecipeMediaValidationException ex)
            {
                return ServiceResult<RecipeMediaDto>.Failure(ex.Code + ":" + ex.Message, ServiceErrorType.Validation);
            }
            catch
            {
                if (url is not null) await _mediaStorage!.DeleteAsync(url, cancellationToken);
                throw;
            }
        }

        public async Task<ServiceResult> RemoveMediaAsync(Guid id, Guid mediaId, Guid currentUserId, bool isAdmin, CancellationToken ct = default)
        {
            var recipe = await _repository.GetByIdAsync(id, ct);
            if (recipe is null) return ServiceResult.Failure("Recipe not found", ServiceErrorType.NotFound);
            if (!isAdmin && recipe.UserId != currentUserId) return ServiceResult.Failure("You can only update your own recipe.", ServiceErrorType.Forbidden);
            var photos = OrderedImages(recipe);
            var media = photos.SingleOrDefault(photo => photo.Id == mediaId);
            if (media is null) return ServiceResult.Failure("Photo not found", ServiceErrorType.NotFound);
            if (photos.Count == 1) return ServiceResult.Failure("The final photo cannot be removed.", ServiceErrorType.Validation);
            recipe.Media.Remove(media);
            Normalize(recipe);
            ResolveCover(recipe);
            await _repository.UpdateAsync(recipe, ct);
            await (_mediaStorage ?? throw new InvalidOperationException("Recipe media storage is not configured.")).DeleteAsync(media.Url, ct);
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> SetMainMediaAsync(Guid id, Guid mediaId, Guid currentUserId, bool isAdmin, CancellationToken ct = default)
        {
            var recipe = await _repository.GetByIdAsync(id, ct);
            if (recipe is null) return ServiceResult.Failure("Recipe not found", ServiceErrorType.NotFound);
            if (!isAdmin && recipe.UserId != currentUserId) return ServiceResult.Failure("You can only update your own recipe.", ServiceErrorType.Forbidden);
            var media = OrderedImages(recipe).SingleOrDefault(photo => photo.Id == mediaId);
            if (media is null) return ServiceResult.Failure("Photo not found", ServiceErrorType.NotFound);
            foreach (var photo in OrderedImages(recipe)) photo.IsMain = photo.Id == mediaId;
            ResolveCover(recipe);
            await _repository.UpdateAsync(recipe, ct);
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> ReorderMediaAsync(Guid id, IReadOnlyList<Guid> ids, Guid currentUserId, bool isAdmin, CancellationToken ct = default)
        {
            var recipe = await _repository.GetByIdAsync(id, ct);
            if (recipe is null) return ServiceResult.Failure("Recipe not found", ServiceErrorType.NotFound);
            if (!isAdmin && recipe.UserId != currentUserId) return ServiceResult.Failure("You can only update your own recipe.", ServiceErrorType.Forbidden);
            var photos = OrderedImages(recipe);
            if (ids.Count != photos.Count || ids.Distinct().Count() != ids.Count || ids.Except(photos.Select(photo => photo.Id)).Any())
                return ServiceResult.Failure("Photo order must contain each recipe photo ID exactly once.", ServiceErrorType.Validation);
            for (var index = 0; index < ids.Count; index++)
            {
                photos.Single(photo => photo.Id == ids[index]).SortOrder = index;
            }
            Normalize(recipe);
            ResolveCover(recipe);
            await _repository.UpdateAsync(recipe, ct);
            return ServiceResult.Success();
        }

        private static List<RecipeMedia> OrderedImages(Recipie recipe) => recipe.Media
            .Where(media => media.MediaType == RecipeMediaType.Image)
            .OrderBy(media => media.SortOrder)
            .ThenBy(media => media.CreatedAt)
            .ToList();

        private static RecipeMediaDto ToMediaDto(RecipeMedia media) => new()
        {
            Id = media.Id, Url = media.Url, ContentType = media.ContentType,
            MediaType = media.MediaType, IsMain = media.IsMain, SortOrder = media.SortOrder
        };

        private static void Normalize(Recipie recipe)
        {
            var photos = OrderedImages(recipe);
            for (var index = 0; index < photos.Count; index++) photos[index].SortOrder = index;
            if (photos.Count == 0) return;
            var main = photos.FirstOrDefault(photo => photo.IsMain) ?? photos[0];
            foreach (var photo in photos) photo.IsMain = photo == main;
        }

        private static void ResolveCover(Recipie recipe)
        {
            var photos = OrderedImages(recipe);
            var cover = photos.FirstOrDefault(photo => photo.IsMain) ?? photos.FirstOrDefault();
            recipe.ImageUrl = cover?.Url;
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
