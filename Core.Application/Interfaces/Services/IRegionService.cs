using Core.Application.Common;
using Core.Application.DTO.Regions;

namespace Core.Application.Interfaces.Services;

public interface IRegionService
{
    Task<ServiceResult<RegionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<RegionDto>> CreateAsync(CreateRegionDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult<RegionDto>> UpdateAsync(Guid id, UpdateRegionDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
