using Core.Application.DTO.Categories;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;

namespace Core.Application.UseCases.Categories;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyCollection<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _repository.GetAllAsync(cancellationToken);
    }
}
