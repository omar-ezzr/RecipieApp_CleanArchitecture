using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.DTO;
using Core.Application.DTO.Recipe;
using Core.Domain.Entities;

namespace Core.Application.Interfaces
{
    public interface IRecipeRepository
    {
        Task<Recipie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken = default);
        Task<bool> CuisineExistsAsync(Guid cuisineId, CancellationToken cancellationToken = default);
        Task<Region?> GetActiveRegionAsync(Guid regionId, CancellationToken cancellationToken = default);
        Task AddAsync(Recipie recipie, CancellationToken cancellationToken = default);
        Task UpdateAsync(Recipie recipie, CancellationToken cancellationToken = default);
        Task DeleteAsync(Recipie recipie, CancellationToken cancellationToken = default);
        Task<(List<Recipie> Items, int Total, int Page, int PageSize, int TotalPages)> GetPagedAsync(
            RecipeQueryParams parameters,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyDictionary<Guid, RecipeLikeStatsDto>> GetLikeStatsAsync(
            IReadOnlyCollection<Guid> recipeIds,
            Guid? currentUserId,
            CancellationToken cancellationToken = default);
    }
}
