using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Tenon.Repository.EfCore.Tests.Entities;

namespace Tenon.Repository.EfCore.Tests.Interceptors;

/// <summary>
/// 博客软删除拦截器
/// </summary>
public class BlogSoftDeleteInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            HandleBlogSoftDelete(eventData.Context);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            HandleBlogSoftDelete(eventData.Context);
        }
        return base.SavingChanges(eventData, result);
    }

    private void HandleBlogSoftDelete(DbContext context)
    {
        var entries = context.ChangeTracker.Entries<Blog>()
            .Where(e => e.State != EntityState.Deleted && e.Entity.IsDeleted)
            .ToList();

        foreach (var entry in entries)
        {
            var comments = context.Set<BlogComment>()
                .IgnoreQueryFilters()
                .Where(c => c.BlogId == entry.Entity.Id)
                .ToList();

            foreach (var comment in comments)
            {
                comment.IsDeleted = true;
                comment.DeletedAt = DateTimeOffset.UtcNow;
                comment.DeletedBy = entry.Entity.DeletedBy;
            }
        }
    }
} 