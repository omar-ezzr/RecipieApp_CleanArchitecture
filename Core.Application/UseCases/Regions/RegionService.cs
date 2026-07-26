using Core.Application.Common;
using Core.Application.DTO.Regions;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Application.UseCases.Cuisines;
using Core.Domain.Entities;

namespace Core.Application.UseCases.Regions;

public sealed class RegionService : IRegionService
{
    private readonly ICuisineRepository _cuisineRepository;
    private readonly IRegionRepository _regionRepository;

    public RegionService(ICuisineRepository cuisineRepository, IRegionRepository regionRepository)
    {
        _cuisineRepository = cuisineRepository;
        _regionRepository = regionRepository;
    }

    public async Task<ServiceResult<RegionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var region = await _regionRepository.GetByIdAsync(id, cancellationToken);

        return region is null
            ? ServiceResult<RegionDto>.Failure("Region not found", ServiceErrorType.NotFound)
            : ServiceResult<RegionDto>.Success(await MapAsync(region, cancellationToken));
    }

    public async Task<ServiceResult<RegionDto>> CreateAsync(CreateRegionDto dto, CancellationToken cancellationToken = default)
    {
        var validation = Validate(dto.Name, dto.CuisineId, dto.Description, dto.ImageUrl);
        if (validation is not null)
        {
            return ServiceResult<RegionDto>.Failure(validation, ServiceErrorType.Validation);
        }

        var cuisine = await _cuisineRepository.GetByIdAsync(dto.CuisineId, cancellationToken);
        if (cuisine is null)
        {
            return ServiceResult<RegionDto>.Failure("Cuisine not found", ServiceErrorType.NotFound);
        }

        var name = Normalize(dto.Name)!;
        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? await GenerateUniqueSlugAsync(dto.CuisineId, name, null, cancellationToken)
            : CuisineService.Slugify(dto.Slug);

        if (await _regionRepository.ExistsByNameAsync(dto.CuisineId, name, null, cancellationToken))
        {
            return ServiceResult<RegionDto>.Failure("Region name already exists in cuisine", ServiceErrorType.Conflict);
        }

        if (await _regionRepository.ExistsBySlugInCuisineAsync(dto.CuisineId, slug, null, cancellationToken))
        {
            return ServiceResult<RegionDto>.Failure("Region slug already exists in cuisine", ServiceErrorType.Conflict);
        }

        var region = new Region
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            Description = NormalizeOptional(dto.Description),
            CuisineId = dto.CuisineId,
            Cuisine = cuisine,
            ImageUrl = NormalizeOptional(dto.ImageUrl),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _regionRepository.AddAsync(region, cancellationToken);
        await _regionRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult<RegionDto>.Success(await MapAsync(region, cancellationToken));
    }

    public async Task<ServiceResult<RegionDto>> UpdateAsync(Guid id, UpdateRegionDto dto, CancellationToken cancellationToken = default)
    {
        var region = await _regionRepository.GetByIdAsync(id, cancellationToken);
        if (region is null)
        {
            return ServiceResult<RegionDto>.Failure("Region not found", ServiceErrorType.NotFound);
        }

        var validation = Validate(dto.Name, dto.CuisineId, dto.Description, dto.ImageUrl);
        if (validation is not null)
        {
            return ServiceResult<RegionDto>.Failure(validation, ServiceErrorType.Validation);
        }

        var cuisine = await _cuisineRepository.GetByIdAsync(dto.CuisineId, cancellationToken);
        if (cuisine is null)
        {
            return ServiceResult<RegionDto>.Failure("Cuisine not found", ServiceErrorType.NotFound);
        }

        var name = Normalize(dto.Name)!;
        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? await GenerateUniqueSlugAsync(dto.CuisineId, name, id, cancellationToken)
            : CuisineService.Slugify(dto.Slug);

        if (await _regionRepository.ExistsByNameAsync(dto.CuisineId, name, id, cancellationToken))
        {
            return ServiceResult<RegionDto>.Failure("Region name already exists in cuisine", ServiceErrorType.Conflict);
        }

        if (await _regionRepository.ExistsBySlugInCuisineAsync(dto.CuisineId, slug, id, cancellationToken))
        {
            return ServiceResult<RegionDto>.Failure("Region slug already exists in cuisine", ServiceErrorType.Conflict);
        }

        region.Name = name;
        region.Slug = slug;
        region.Description = NormalizeOptional(dto.Description);
        region.CuisineId = dto.CuisineId;
        region.Cuisine = cuisine;
        region.ImageUrl = NormalizeOptional(dto.ImageUrl);
        region.IsActive = dto.IsActive;

        _regionRepository.Update(region);
        await _regionRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult<RegionDto>.Success(await MapAsync(region, cancellationToken));
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var region = await _regionRepository.GetByIdAsync(id, cancellationToken);
        if (region is null)
        {
            return ServiceResult.Failure("Region not found", ServiceErrorType.NotFound);
        }

        if (await _regionRepository.HasRecipesAsync(id, cancellationToken))
        {
            return ServiceResult.Failure("Region is referenced by recipes", ServiceErrorType.Conflict);
        }

        _regionRepository.Delete(region);
        await _regionRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    private async Task<string> GenerateUniqueSlugAsync(Guid cuisineId, string name, Guid? excludeId, CancellationToken cancellationToken)
    {
        var baseSlug = CuisineService.Slugify(name);
        var slug = baseSlug;
        var suffix = 2;

        while (await _regionRepository.ExistsBySlugInCuisineAsync(cuisineId, slug, excludeId, cancellationToken))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        return slug;
    }

    private async Task<RegionDto> MapAsync(Region region, CancellationToken cancellationToken)
    {
        return new RegionDto
        {
            Id = region.Id,
            Name = region.Name,
            Slug = region.Slug,
            Description = region.Description,
            CuisineId = region.CuisineId,
            CuisineName = region.Cuisine.Name,
            ImageUrl = region.ImageUrl,
            IsActive = region.IsActive,
            CreatedAt = region.CreatedAt,
            RecipeCount = await _regionRepository.CountRecipesAsync(region.Id, cancellationToken)
        };
    }

    private static string? Validate(string? name, Guid cuisineId, string? description, string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Name is required";
        if (name.Trim().Length > 120) return "Name must be 120 characters or fewer";
        if (cuisineId == Guid.Empty) return "Cuisine is required";
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
