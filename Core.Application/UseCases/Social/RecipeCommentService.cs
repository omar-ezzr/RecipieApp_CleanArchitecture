using Core.Application.Common;
using Core.Application.DTO.Social;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Domain.Constants;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Application.UseCases.Social;

public sealed class RecipeCommentService : IRecipeCommentService
{
    private const int MaxContentLength = 1500;
    private readonly IRecipeCommentRepository _commentRepository;
    private readonly INotificationRepository _notificationRepository;

    public RecipeCommentService(IRecipeCommentRepository commentRepository, INotificationRepository notificationRepository)
    {
        _commentRepository = commentRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task<ServiceResult<PagedResult<RecipeCommentDto>>> GetByRecipeAsync(Guid recipeId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var recipe = await _commentRepository.GetRecipeStatusAsync(recipeId, cancellationToken);
        if (!recipe.Exists)
        {
            return ServiceResult<PagedResult<RecipeCommentDto>>.Failure("Recipe was not found.", ServiceErrorType.NotFound);
        }

        return ServiceResult<PagedResult<RecipeCommentDto>>.Success(await _commentRepository.GetByRecipeAsync(recipeId, page, pageSize, cancellationToken));
    }

    public async Task<ServiceResult<RecipeCommentDto>> CreateAsync(Guid currentUserId, Guid recipeId, CreateRecipeCommentDto dto, CancellationToken cancellationToken = default)
    {
        var contentResult = NormalizeContent(dto.Content);
        if (!contentResult.IsSuccess)
        {
            return ServiceResult<RecipeCommentDto>.Failure(contentResult.Error!, contentResult.ErrorType);
        }

        var recipe = await _commentRepository.GetRecipeStatusAsync(recipeId, cancellationToken);
        if (!recipe.Exists)
        {
            return ServiceResult<RecipeCommentDto>.Failure("Recipe was not found.", ServiceErrorType.NotFound);
        }

        var comment = new RecipeComment
        {
            Id = Guid.NewGuid(),
            RecipeId = recipeId,
            UserId = currentUserId,
            Content = contentResult.Value!,
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepository.AddAsync(comment, cancellationToken);
        await _notificationRepository.CreateAsync(recipe.OwnerId!.Value, currentUserId, NotificationType.RecipeComment, recipeId, comment.Id, cancellationToken);
        await _commentRepository.SaveChangesAsync(cancellationToken);

        var created = await _commentRepository.GetDtoByIdAsync(comment.Id, cancellationToken);
        return ServiceResult<RecipeCommentDto>.Success(created!);
    }

    public async Task<ServiceResult<RecipeCommentDto>> UpdateAsync(Guid currentUserId, Guid commentId, UpdateRecipeCommentDto dto, CancellationToken cancellationToken = default)
    {
        var contentResult = NormalizeContent(dto.Content);
        if (!contentResult.IsSuccess)
        {
            return ServiceResult<RecipeCommentDto>.Failure(contentResult.Error!, contentResult.ErrorType);
        }

        var comment = await _commentRepository.GetByIdAsync(commentId, track: true, cancellationToken);
        if (comment is null)
        {
            return ServiceResult<RecipeCommentDto>.Failure("Comment was not found.", ServiceErrorType.NotFound);
        }

        if (comment.UserId != currentUserId)
        {
            return ServiceResult<RecipeCommentDto>.Failure("You can only update your own comment.", ServiceErrorType.Forbidden);
        }

        comment.Content = contentResult.Value!;
        comment.UpdatedAt = DateTime.UtcNow;
        await _commentRepository.SaveChangesAsync(cancellationToken);

        var updated = await _commentRepository.GetDtoByIdAsync(comment.Id, cancellationToken);
        return ServiceResult<RecipeCommentDto>.Success(updated!);
    }

    public async Task<ServiceResult> DeleteAsync(Guid currentUserId, bool isAdmin, Guid commentId, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId, track: true, cancellationToken);
        if (comment is null)
        {
            return ServiceResult.Failure("Comment was not found.", ServiceErrorType.NotFound);
        }

        if (!isAdmin && comment.UserId != currentUserId)
        {
            return ServiceResult.Failure("You can only delete your own comment.", ServiceErrorType.Forbidden);
        }

        await _commentRepository.DeleteAsync(comment);
        await _commentRepository.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    private static ServiceResult<string> NormalizeContent(string? content)
    {
        var normalized = content?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return ServiceResult<string>.Failure("Comment content is required.", ServiceErrorType.Validation);
        }

        if (normalized.Length > MaxContentLength)
        {
            return ServiceResult<string>.Failure("Comment content must be 1500 characters or fewer.", ServiceErrorType.Validation);
        }

        return ServiceResult<string>.Success(normalized);
    }
}
