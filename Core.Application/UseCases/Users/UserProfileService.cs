using Core.Application.Common;
using Core.Application.DTO;
using Core.Application.DTO.Recipe;
using Core.Application.DTO.Users;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;

namespace Core.Application.UseCases.Users;

public sealed class UserProfileService : IUserProfileService
{
    private const int DisplayNameMaxLength = 100;
    private const int BioMaxLength = 1000;
    private const int AvatarUrlMaxLength = 2048;
    private const int CountryCodeMaxLength = 10;
    private readonly IUserProfileRepository _repository;

    public UserProfileService(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResult<PublicUserProfileDto>> GetPublicProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetPublicProfileAsync(userId, cancellationToken);
        return profile is null
            ? ServiceResult<PublicUserProfileDto>.Failure("User profile was not found.", ServiceErrorType.NotFound)
            : ServiceResult<PublicUserProfileDto>.Success(profile);
    }

    public async Task<ServiceResult<PagedResult<RecipieDto>>> GetUserRecipesAsync(
        Guid userId,
        RecipeQueryParams parameters,
        CancellationToken cancellationToken = default)
    {
        if (await _repository.GetPublicProfileAsync(userId, cancellationToken) is null)
        {
            return ServiceResult<PagedResult<RecipieDto>>.Failure("User profile was not found.", ServiceErrorType.NotFound);
        }

        var recipes = await _repository.GetUserRecipesAsync(userId, parameters, cancellationToken);
        return ServiceResult<PagedResult<RecipieDto>>.Success(recipes);
    }

    public async Task<ServiceResult<PublicUserProfileDto>> UpdateCurrentProfileAsync(
        Guid currentUserId,
        UpdatePublicUserProfileDto dto,
        CancellationToken cancellationToken = default)
    {
        var displayName = dto.DisplayName?.Trim();
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return ServiceResult<PublicUserProfileDto>.Failure("Display name is required.", ServiceErrorType.Validation);
        }

        if (displayName.Length > DisplayNameMaxLength)
        {
            return ServiceResult<PublicUserProfileDto>.Failure("Display name must be 100 characters or fewer.", ServiceErrorType.Validation);
        }

        var bio = NormalizeOptional(dto.Bio);
        if (bio?.Length > BioMaxLength)
        {
            return ServiceResult<PublicUserProfileDto>.Failure("Bio must be 1000 characters or fewer.", ServiceErrorType.Validation);
        }

        var avatarUrl = NormalizeOptional(dto.AvatarUrl);
        if (avatarUrl?.Length > AvatarUrlMaxLength || !IsValidAvatarUrl(avatarUrl))
        {
            return ServiceResult<PublicUserProfileDto>.Failure("Avatar URL must be an http/https URL or a backend-relative path.", ServiceErrorType.Validation);
        }

        var countryCode = NormalizeOptional(dto.CountryCode)?.ToUpperInvariant();
        if (countryCode?.Length > CountryCodeMaxLength)
        {
            return ServiceResult<PublicUserProfileDto>.Failure("Country code must be 10 characters or fewer.", ServiceErrorType.Validation);
        }

        var user = await _repository.GetActiveUserForUpdateAsync(currentUserId, cancellationToken);
        if (user is null)
        {
            return ServiceResult<PublicUserProfileDto>.Failure("User profile was not found.", ServiceErrorType.NotFound);
        }

        user.DisplayName = displayName;
        user.Bio = bio;
        user.AvatarUrl = avatarUrl;
        user.CountryCode = countryCode;
        await _repository.SaveChangesAsync(cancellationToken);

        var profile = await _repository.GetPublicProfileAsync(currentUserId, cancellationToken);
        return ServiceResult<PublicUserProfileDto>.Success(profile!);
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static bool IsValidAvatarUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return value.StartsWith("/", StringComparison.Ordinal)
            || Uri.TryCreate(value, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
