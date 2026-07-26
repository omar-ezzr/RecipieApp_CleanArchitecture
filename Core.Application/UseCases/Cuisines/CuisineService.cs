using System.Globalization;
using System.Text;
using Core.Application.Common;
using Core.Application.DTO.Cuisines;
using Core.Application.DTO.Regions;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Domain.Entities;

namespace Core.Application.UseCases.Cuisines;

public sealed class CuisineService : ICuisineService
{
    private readonly ICuisineRepository _cuisineRepository;
    private readonly IRegionRepository _regionRepository;

    public CuisineService(ICuisineRepository cuisineRepository, IRegionRepository regionRepository)
    {
        _cuisineRepository = cuisineRepository;
        _regionRepository = regionRepository;
    }

    public Task<List<CuisineDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _cuisineRepository.GetAllAsync(activeOnly: true, cancellationToken);
    }

    public async Task<ServiceResult<CuisineDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cuisine = await _cuisineRepository.GetByIdAsync(id, cancellationToken);

        return cuisine is null
            ? ServiceResult<CuisineDto>.Failure("Cuisine not found", ServiceErrorType.NotFound)
            : ServiceResult<CuisineDto>.Success(await MapAsync(cuisine, cancellationToken));
    }

    public async Task<List<RegionDto>> GetRegionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _regionRepository.GetAllByCuisineAsync(id, activeOnly: true, cancellationToken);
    }

    public async Task<ServiceResult<CuisineDto>> CreateAsync(CreateCuisineDto dto, CancellationToken cancellationToken = default)
    {
        var validation = Validate(dto.Name, dto.CountryCode, dto.Description, dto.ImageUrl);
        if (validation is not null)
        {
            return ServiceResult<CuisineDto>.Failure(validation, ServiceErrorType.Validation);
        }

        var name = Normalize(dto.Name)!;
        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? await GenerateUniqueSlugAsync(name, null, cancellationToken)
            : Slugify(dto.Slug);

        if (await _cuisineRepository.ExistsByNameAsync(name, null, cancellationToken))
        {
            return ServiceResult<CuisineDto>.Failure("Cuisine name already exists", ServiceErrorType.Conflict);
        }

        if (await _cuisineRepository.ExistsBySlugAsync(slug, null, cancellationToken))
        {
            return ServiceResult<CuisineDto>.Failure("Cuisine slug already exists", ServiceErrorType.Conflict);
        }

        var cuisine = new Cuisine
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            Description = NormalizeOptional(dto.Description),
            CountryCode = Normalize(dto.CountryCode)!.ToUpperInvariant(),
            ImageUrl = NormalizeOptional(dto.ImageUrl),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _cuisineRepository.AddAsync(cuisine, cancellationToken);
        await _cuisineRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult<CuisineDto>.Success(await MapAsync(cuisine, cancellationToken));
    }

    public async Task<ServiceResult<CuisineDto>> UpdateAsync(Guid id, UpdateCuisineDto dto, CancellationToken cancellationToken = default)
    {
        var cuisine = await _cuisineRepository.GetByIdAsync(id, cancellationToken);
        if (cuisine is null)
        {
            return ServiceResult<CuisineDto>.Failure("Cuisine not found", ServiceErrorType.NotFound);
        }

        var validation = Validate(dto.Name, dto.CountryCode, dto.Description, dto.ImageUrl);
        if (validation is not null)
        {
            return ServiceResult<CuisineDto>.Failure(validation, ServiceErrorType.Validation);
        }

        var name = Normalize(dto.Name)!;
        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? await GenerateUniqueSlugAsync(name, id, cancellationToken)
            : Slugify(dto.Slug);

        if (await _cuisineRepository.ExistsByNameAsync(name, id, cancellationToken))
        {
            return ServiceResult<CuisineDto>.Failure("Cuisine name already exists", ServiceErrorType.Conflict);
        }

        if (await _cuisineRepository.ExistsBySlugAsync(slug, id, cancellationToken))
        {
            return ServiceResult<CuisineDto>.Failure("Cuisine slug already exists", ServiceErrorType.Conflict);
        }

        cuisine.Name = name;
        cuisine.Slug = slug;
        cuisine.Description = NormalizeOptional(dto.Description);
        cuisine.CountryCode = Normalize(dto.CountryCode)!.ToUpperInvariant();
        cuisine.ImageUrl = NormalizeOptional(dto.ImageUrl);
        cuisine.IsActive = dto.IsActive;

        _cuisineRepository.Update(cuisine);
        await _cuisineRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult<CuisineDto>.Success(await MapAsync(cuisine, cancellationToken));
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cuisine = await _cuisineRepository.GetByIdAsync(id, cancellationToken);
        if (cuisine is null)
        {
            return ServiceResult.Failure("Cuisine not found", ServiceErrorType.NotFound);
        }

        if (await _cuisineRepository.HasRecipesAsync(id, cancellationToken))
        {
            return ServiceResult.Failure("Cuisine is referenced by recipes", ServiceErrorType.Conflict);
        }

        if (await _cuisineRepository.HasRegionsAsync(id, cancellationToken))
        {
            return ServiceResult.Failure("Cuisine is referenced by regions", ServiceErrorType.Conflict);
        }

        _cuisineRepository.Delete(cuisine);
        await _cuisineRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    private async Task<CuisineDto> MapAsync(Cuisine cuisine, CancellationToken cancellationToken)
    {
        return new CuisineDto
        {
            Id = cuisine.Id,
            Name = cuisine.Name,
            Slug = cuisine.Slug,
            Description = cuisine.Description,
            CountryCode = cuisine.CountryCode,
            ImageUrl = cuisine.ImageUrl,
            IsActive = cuisine.IsActive,
            CreatedAt = cuisine.CreatedAt,
            RegionCount = await _cuisineRepository.CountRegionsAsync(cuisine.Id, cancellationToken),
            RecipeCount = await _cuisineRepository.CountRecipesAsync(cuisine.Id, cancellationToken)
        };
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, Guid? excludeId, CancellationToken cancellationToken)
    {
        var baseSlug = Slugify(name);
        var slug = baseSlug;
        var suffix = 2;

        while (await _cuisineRepository.ExistsBySlugAsync(slug, excludeId, cancellationToken))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        return slug;
    }

    internal static string Slugify(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        var previousHyphen = false;

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousHyphen = false;
            }
            else if (!previousHyphen)
            {
                builder.Append('-');
                previousHyphen = true;
            }
        }

        return builder.ToString().Trim('-') is { Length: > 0 } slug ? slug : "culture";
    }

    private static string? Validate(string? name, string? countryCode, string? description, string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Name is required";
        if (name.Trim().Length > 120) return "Name must be 120 characters or fewer";
        if (string.IsNullOrWhiteSpace(countryCode)) return "Country code is required";
        if (countryCode.Trim().Length is < 2 or > 3) return "Country code must be 2 or 3 characters";
        if (description?.Trim().Length > 1000) return "Description must be 1,000 characters or fewer";
        if (imageUrl?.Trim().Length > 2048) return "Image URL must be 2,048 characters or fewer";
        if (!IsHttpUrl(imageUrl)) return "Image URL must be an absolute HTTP or HTTPS URL";
        return null;
    }

    private static bool IsHttpUrl(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            || (Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps));
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string? Normalize(string? value) => NormalizeOptional(value);
}
