using Core.Application.DTO.Regions;
using Core.Domain.Entities;

namespace Core.Application.Interfaces.Repositories;

public interface IRegionRepository
{
    Task<List<RegionDto>> GetAllByCuisineAsync(Guid cuisineId, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<Region?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Region?> GetBySlugAsync(Guid cuisineId, string slug, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(Guid cuisineId, string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySlugInCuisineAsync(Guid cuisineId, string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Region region, CancellationToken cancellationToken = default);
    void Update(Region region);
    void Delete(Region region);
    Task<bool> HasRecipesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountRecipesAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
