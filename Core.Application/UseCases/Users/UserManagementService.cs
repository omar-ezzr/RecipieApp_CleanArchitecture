using Core.Application.Common;
using Core.Application.DTO.Users;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Domain.Constants;
using UserEntity = Core.Domain.Entities.Users;

namespace Core.Application.UseCases.Users;

public sealed class UserManagementService : IUserManagementService
{
    private readonly IUserRepository _repository;
    private readonly IPasswordService _passwordService;

    public UserManagementService(IUserRepository repository, IPasswordService passwordService)
    {
        _repository = repository;
        _passwordService = passwordService;
    }

    public async Task<ServiceResult<PagedUsersDto>> GetPagedAsync(
        UserQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(parameters.Role) && !AppRoles.TryNormalize(parameters.Role, out _))
        {
            return ServiceResult<PagedUsersDto>.Failure("Role must be User, Operator, or Admin.", ServiceErrorType.Validation);
        }

        var result = await _repository.GetPagedAsync(parameters, cancellationToken);

        return ServiceResult<PagedUsersDto>.Success(new PagedUsersDto
        {
            Items = result.Items.Select(Map).ToList(),
            Total = result.Total,
            Page = result.Page,
            PageSize = result.PageSize
        });
    }

    public async Task<ServiceResult<UserAccountDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken: cancellationToken);

        return user is null
            ? ServiceResult<UserAccountDto>.Failure("Account was not found.", ServiceErrorType.NotFound)
            : ServiceResult<UserAccountDto>.Success(Map(user));
    }

    public async Task<ServiceResult<UserAccountDto>> CreateAsync(
        CreateUserAccountDto dto,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(dto.Email);

        if (!AppRoles.TryNormalize(dto.Role, out var role))
        {
            return ServiceResult<UserAccountDto>.Failure("Role must be User, Operator, or Admin.", ServiceErrorType.Validation);
        }

        if (await _repository.EmailExistsAsync(email, cancellationToken))
        {
            return ServiceResult<UserAccountDto>.Failure("An account with this email already exists.", ServiceErrorType.Conflict);
        }

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = _passwordService.Hash(dto.Password),
            Role = role,
            IsActive = true
        };

        await _repository.AddAsync(user, cancellationToken);

        try
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            return ServiceResult<UserAccountDto>.Failure("An account with this email already exists.", ServiceErrorType.Conflict);
        }

        return ServiceResult<UserAccountDto>.Success(Map(user));
    }

    public async Task<ServiceResult<UserAccountDto>> UpdateRoleAsync(
        Guid currentUserId,
        Guid targetUserId,
        UpdateUserRoleDto dto,
        CancellationToken cancellationToken = default)
    {
        if (currentUserId == targetUserId)
        {
            return ServiceResult<UserAccountDto>.Failure("Administrators cannot change their own role.", ServiceErrorType.Validation);
        }

        if (!AppRoles.TryNormalize(dto.Role, out var role))
        {
            return ServiceResult<UserAccountDto>.Failure("Role must be User, Operator, or Admin.", ServiceErrorType.Validation);
        }

        var user = await _repository.GetByIdAsync(targetUserId, track: true, cancellationToken);

        if (user is null)
        {
            return ServiceResult<UserAccountDto>.Failure("Account was not found.", ServiceErrorType.NotFound);
        }

        if (user.Role == AppRoles.Admin && user.IsActive && role != AppRoles.Admin &&
            await _repository.CountActiveAdminsAsync(cancellationToken) <= 1)
        {
            return ServiceResult<UserAccountDto>.Failure("The final active Admin cannot be changed to another role.", ServiceErrorType.Conflict);
        }

        user.Role = role;
        ClearRefreshToken(user);

        _repository.Update(user);
        await _repository.SaveChangesAsync(cancellationToken);

        return ServiceResult<UserAccountDto>.Success(Map(user));
    }

    public async Task<ServiceResult<UserAccountDto>> UpdateStatusAsync(
        Guid currentUserId,
        Guid targetUserId,
        UpdateUserStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        if (currentUserId == targetUserId && !dto.IsActive)
        {
            return ServiceResult<UserAccountDto>.Failure("Administrators cannot deactivate their own account.", ServiceErrorType.Validation);
        }

        var user = await _repository.GetByIdAsync(targetUserId, track: true, cancellationToken);

        if (user is null)
        {
            return ServiceResult<UserAccountDto>.Failure("Account was not found.", ServiceErrorType.NotFound);
        }

        if (user.Role == AppRoles.Admin && user.IsActive && !dto.IsActive &&
            await _repository.CountActiveAdminsAsync(cancellationToken) <= 1)
        {
            return ServiceResult<UserAccountDto>.Failure("The final active Admin cannot be deactivated.", ServiceErrorType.Conflict);
        }

        user.IsActive = dto.IsActive;

        if (!dto.IsActive)
        {
            ClearRefreshToken(user);
        }

        _repository.Update(user);
        await _repository.SaveChangesAsync(cancellationToken);

        return ServiceResult<UserAccountDto>.Success(Map(user));
    }

    public async Task<ServiceResult> DeleteAsync(
        Guid currentUserId,
        Guid targetUserId,
        CancellationToken cancellationToken = default)
    {
        if (currentUserId == targetUserId)
        {
            return ServiceResult.Failure("Administrators cannot delete their own account.", ServiceErrorType.Validation);
        }

        var user = await _repository.GetByIdAsync(targetUserId, track: true, cancellationToken);

        if (user is null)
        {
            return ServiceResult.Failure("Account was not found.", ServiceErrorType.NotFound);
        }

        if (user.Role == AppRoles.Admin && user.IsActive &&
            await _repository.CountActiveAdminsAsync(cancellationToken) <= 1)
        {
            return ServiceResult.Failure("The final active Admin cannot be deleted.", ServiceErrorType.Conflict);
        }

        if (await _repository.HasFavoritesOrReviewsAsync(targetUserId, cancellationToken))
        {
            return ServiceResult.Failure("The account has related favorites or reviews. Deactivate it instead.", ServiceErrorType.Conflict);
        }

        _repository.Delete(user);
        await _repository.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    private static UserAccountDto Map(UserEntity user)
    {
        return new UserAccountDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive
        };
    }

    private static void ClearRefreshToken(UserEntity user)
    {
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
