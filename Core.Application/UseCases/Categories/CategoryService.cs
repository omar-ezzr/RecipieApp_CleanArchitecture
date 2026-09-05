using Core.Application.Common;
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

    public async Task<ServiceResult<CategoryDto>> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default) { var category = new Core.Domain.Entities.Category { Id = Guid.NewGuid(), Name = dto.Name.Trim() }; await _repository.AddAsync(category, cancellationToken); return ServiceResult<CategoryDto>.Success(new CategoryDto { Id = category.Id, Name = category.Name }); }
    public async Task<ServiceResult<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken = default) { var category = await _repository.GetByIdAsync(id, cancellationToken); if (category is null) return ServiceResult<CategoryDto>.Failure("Category was not found.", ServiceErrorType.NotFound); category.Name = dto.Name.Trim(); await _repository.SaveChangesAsync(cancellationToken); return ServiceResult<CategoryDto>.Success(new CategoryDto { Id = category.Id, Name = category.Name }); }
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default) { var category = await _repository.GetByIdAsync(id, cancellationToken); if (category is null) return ServiceResult.Failure("Category was not found.", ServiceErrorType.NotFound); if (await _repository.IsUsedAsync(id, cancellationToken)) return ServiceResult.Failure("Categories used by recipes cannot be deleted.", ServiceErrorType.Conflict); await _repository.DeleteAsync(category, cancellationToken); return ServiceResult.Success(); }
}
