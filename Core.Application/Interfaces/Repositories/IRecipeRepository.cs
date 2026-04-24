using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.DTO;
using Core.Domain.Entities;

namespace Core.Application.Interfaces
{
    public interface IRecipeRepository
    { Task<IEnumerable<Recipie>> GetAllAsync();
    Task<Recipie?> GetByIdAsync(Guid id);
    Task AddAsync(Recipie recipie);
    Task UpdateAsync(Recipie recipie);
    Task DeleteAsync(Recipie recipie);
    Task<(List<Recipie>, int)> GetPagedAsync(RecipeQueryParams parameters);
    }
}
