using Core.Application.Common;
using Core.Application.DTO.Cuisines;
using Core.Application.DTO.Regions;

namespace Core.Application.Interfaces.Services;

public interface ICuisineService
{
    Task<List<CuisineDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<CuisineDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<RegionDto>> GetRegionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<CuisineDto>> CreateAsync(CreateCuisineDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<CuisineDto>> UpdateAsync(Guid id, UpdateCuisineDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
