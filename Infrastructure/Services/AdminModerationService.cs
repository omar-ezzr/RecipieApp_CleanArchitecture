using Core.Application.Common;
using Core.Application.DTO.Admin;
using Core.Application.Interfaces.Services;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services;
public sealed class AdminModerationService(AppDbContext context) : IAdminModerationService
{
    public Task<PagedResult<AdminRecipeListItemDto>> GetRecipesAsync(AdminListQuery q, CancellationToken ct = default) => Page<Core.Domain.Entities.Recipie, AdminRecipeListItemDto>(context.Recipies.AsNoTracking(), q, r => string.IsNullOrWhiteSpace(q.Search) || r.Title.Contains(q.Search), r => new() { Id=r.Id, Title=r.Title, Author=r.User.DisplayName, Category=r.Category.Name, Cuisine=r.Cuisine.Name, CreatedAt=r.CreatedAt }, ct);
    public Task<PagedResult<AdminCommentListItemDto>> GetCommentsAsync(AdminListQuery q, CancellationToken ct = default) => Page<Core.Domain.Entities.RecipeComment, AdminCommentListItemDto>(context.RecipeComments.AsNoTracking(), q, c => string.IsNullOrWhiteSpace(q.Search) || c.Content.Contains(q.Search) || c.User.DisplayName.Contains(q.Search), c => new() { Id=c.Id, RecipeId=c.RecipeId, Content=c.Content, Author=c.User.DisplayName, RecipeTitle=c.Recipe.Title, CreatedAt=c.CreatedAt }, ct);
    public Task<PagedResult<AdminReviewListItemDto>> GetReviewsAsync(AdminListQuery q, CancellationToken ct = default) => Page<Core.Domain.Entities.RecipeReview, AdminReviewListItemDto>(context.RecipeReviews.AsNoTracking(), q, r => string.IsNullOrWhiteSpace(q.Search) || r.Comment.Contains(q.Search) || r.User.DisplayName.Contains(q.Search), r => new() { Id=r.Id, RecipeId=r.RecipeId, Rating=r.Rating, Comment=r.Comment, Author=r.User.DisplayName, RecipeTitle=r.Recipe.Title, CreatedAt=r.CreatedAt }, ct);
    private static async Task<PagedResult<T>> Page<E,T>(IQueryable<E> source, AdminListQuery input, System.Linq.Expressions.Expression<Func<E,bool>> filter, System.Linq.Expressions.Expression<Func<E,T>> select, CancellationToken ct) where E:class { var page=Math.Max(1,input.Page); var size=Math.Clamp(input.PageSize,1,100); var q=source.Where(filter); var total=await q.CountAsync(ct); var items=await q.OrderByDescending(x=>EF.Property<DateTime>(x,"CreatedAt")).ThenByDescending(x=>EF.Property<Guid>(x,"Id")).Skip((page-1)*size).Take(size).Select(select).ToListAsync(ct); return new PagedResult<T>{Items=items,Total=total,Page=page,PageSize=size,TotalPages=total==0?0:(int)Math.Ceiling(total/(double)size)}; }
}
