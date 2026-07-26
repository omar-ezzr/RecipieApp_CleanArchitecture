using Core.Application.DTO.Cuisines;
using Core.Domain.Entities;

namespace Core.Application.Interfaces.Repositories;

public interface ICuisineRepository
{
    Task<List<CuisineDto>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<Cuisine?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Cuisine?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySlugAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Cuisine cuisine, CancellationToken cancellationToken = default);
    void Update(Cuisine cuisine);
    void Delete(Cuisine cuisine);
    Task<bool> HasRecipesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HasRegionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountRecipesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountRegionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
