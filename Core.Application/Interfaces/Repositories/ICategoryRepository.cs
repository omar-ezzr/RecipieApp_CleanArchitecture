using Core.Application.DTO.Categories;

namespace Core.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<IReadOnlyCollection<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
