using Core.Application.Common;
using Core.Application.DTO.Social;
using Core.Application.DTO.Users;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Application.UseCases.Social;

public sealed class RecipeLikeService : IRecipeLikeService
{
    private readonly IRecipeLikeRepository _likeRepository;
    private readonly INotificationRepository _notificationRepository;

    public RecipeLikeService(IRecipeLikeRepository likeRepository, INotificationRepository notificationRepository)
    {
        _likeRepository = likeRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task<ServiceResult> LikeAsync(Guid currentUserId, Guid recipeId, CancellationToken cancellationToken = default)
    {
        var recipe = await _likeRepository.GetRecipeStatusAsync(recipeId, cancellationToken);
        if (!recipe.Exists)
        {
            return ServiceResult.Failure("Recipe was not found.", ServiceErrorType.NotFound);
        }

        if (await _likeRepository.ExistsAsync(currentUserId, recipeId, cancellationToken))
        {
            return ServiceResult.Failure("Recipe is already liked.", ServiceErrorType.Conflict);
        }

        await _likeRepository.AddAsync(new RecipeLike
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            RecipeId = recipeId,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
        await _notificationRepository.CreateAsync(recipe.OwnerId!.Value, currentUserId, NotificationType.RecipeLike, recipeId, null, cancellationToken);
        await _likeRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UnlikeAsync(Guid currentUserId, Guid recipeId, CancellationToken cancellationToken = default)
    {
        var recipe = await _likeRepository.GetRecipeStatusAsync(recipeId, cancellationToken);
        if (!recipe.Exists)
        {
            return ServiceResult.Failure("Recipe was not found.", ServiceErrorType.NotFound);
        }

        await _likeRepository.RemoveAsync(currentUserId, recipeId, cancellationToken);
        await _likeRepository.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult<LikeStatusDto>> GetStatusAsync(Guid currentUserId, Guid recipeId, CancellationToken cancellationToken = default)
    {
        var recipe = await _likeRepository.GetRecipeStatusAsync(recipeId, cancellationToken);
        if (!recipe.Exists)
        {
            return ServiceResult<LikeStatusDto>.Failure("Recipe was not found.", ServiceErrorType.NotFound);
        }

        return ServiceResult<LikeStatusDto>.Success(new LikeStatusDto
        {
            IsLiked = await _likeRepository.ExistsAsync(currentUserId, recipeId, cancellationToken),
            LikeCount = await _likeRepository.CountAsync(recipeId, cancellationToken)
        });
    }

    public async Task<ServiceResult<PagedResult<UserSummaryDto>>> GetLikesAsync(Guid recipeId, Guid? currentUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var recipe = await _likeRepository.GetRecipeStatusAsync(recipeId, cancellationToken);
        if (!recipe.Exists)
        {
            return ServiceResult<PagedResult<UserSummaryDto>>.Failure("Recipe was not found.", ServiceErrorType.NotFound);
        }

        return ServiceResult<PagedResult<UserSummaryDto>>.Success(await _likeRepository.GetRecipeLikesAsync(recipeId, currentUserId, page, pageSize, cancellationToken));
    }
}
