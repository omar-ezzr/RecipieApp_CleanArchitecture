using Core.Application.DTO.Categories;

namespace Core.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<IReadOnlyCollection<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
