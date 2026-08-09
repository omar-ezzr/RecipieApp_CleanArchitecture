using Core.Application.Common;
using Core.Application.DTO;
using Core.Application.DTO.Recipe;
using Core.Application.DTO.Users;
using Core.Application.Interfaces.Repositories;
using Core.Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class UserProfileRepository : IUserProfileRepository
{
    private readonly AppDbContext _context;

    public UserProfileRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PublicUserProfileDto?> GetPublicProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(user => user.Id == userId && user.IsActive)
            .Select(user => new PublicUserProfileDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl,
                CountryCode = user.CountryCode,
                FollowerCount = _context.UserFollows.Count(follow => follow.FollowedUserId == user.Id),
                FollowingCount = _context.UserFollows.Count(follow => follow.FollowerUserId == user.Id),
                RecipeCount = _context.Recipies.Count(recipe => recipe.UserId == user.Id)
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Users?> GetActiveUserForUpdateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.Id == userId && user.IsActive, cancellationToken);
    }

    public async Task<PagedResult<RecipieDto>> GetUserRecipesAsync(
        Guid userId,
        RecipeQueryParams parameters,
        CancellationToken cancellationToken = default)
    {
        var page = parameters.Page < 1 ? 1 : parameters.Page;
        var pageSize = parameters.PageSize < 1 ? 10 : Math.Min(parameters.PageSize, 50);

        var query = _context.Recipies
            .AsNoTracking()
            .Where(recipe => recipe.UserId == userId)
            .OrderByDescending(recipe => recipe.CreatedAt)
            .ThenByDescending(recipe => recipe.Id);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(recipe => new RecipieDto
            {
                Id = recipe.Id,
                Title = recipe.Title,
                Description = recipe.Description,
                PreparationTimeMinutes = recipe.PreparationTimeMinutes,
                CategoryId = recipe.CategoryId,
                Category = recipe.Category.Name,
                CuisineId = recipe.CuisineId,
                CuisineName = recipe.Cuisine.Name,
                CuisineSlug = recipe.Cuisine.Slug,
                RegionId = recipe.RegionId,
                RegionName = recipe.Region == null ? null : recipe.Region.Name,
                RegionSlug = recipe.Region == null ? null : recipe.Region.Slug,
                Author = new AuthorDto
                {
                    Id = recipe.UserId,
                    DisplayName = recipe.User.DisplayName,
                    AvatarUrl = recipe.User.AvatarUrl
                },
                ImageUrl = recipe.ImageUrl,
                Difficulty = recipe.Difficulty,
                TraditionalName = recipe.TraditionalName,
                OriginDescription = recipe.OriginDescription,
                IsTraditional = recipe.IsTraditional,
                ServingOccasion = recipe.ServingOccasion,
                Ingredients = recipe.Ingredients
                    .Select(ingredient => new CreateIngredientDto { Name = ingredient.Name, Quantity = ingredient.Quantity })
                    .ToList(),
                Steps = recipe.Steps
                    .OrderBy(step => step.StepNumber)
                    .Select(step => new CreateRecipeStepDto { StepNumber = step.StepNumber, Instruction = step.Instruction })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<RecipieDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
