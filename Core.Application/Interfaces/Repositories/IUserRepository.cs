using Core.Application.DTO.Users;
using Core.Domain.Entities;

namespace Core.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<(IReadOnlyCollection<Users> Items, int Total, int Page, int PageSize)> GetPagedAsync(
        UserQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<Users?> GetByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default);
    Task<Users?> GetByEmailAsync(string normalizedEmail, bool track = false, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default);
    Task<int> CountActiveAdminsAsync(CancellationToken cancellationToken = default);
    Task<bool> HasFavoritesOrReviewsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Users user, CancellationToken cancellationToken = default);
    void Update(Users user);
    void Delete(Users user);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
