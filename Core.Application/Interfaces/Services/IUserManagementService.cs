using Core.Application.Common;
using Core.Application.DTO.Users;

namespace Core.Application.Interfaces.Services;

public interface IUserManagementService
{
    Task<ServiceResult<PagedUsersDto>> GetPagedAsync(UserQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<ServiceResult<UserAccountDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<UserAccountDto>> CreateAsync(CreateUserAccountDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<UserAccountDto>> UpdateRoleAsync(Guid currentUserId, Guid targetUserId, UpdateUserRoleDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<UserAccountDto>> UpdateStatusAsync(Guid currentUserId, Guid targetUserId, UpdateUserStatusDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAsync(Guid currentUserId, Guid targetUserId, CancellationToken cancellationToken = default);
}
