using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Tenon.Repository.EfCore.Tests.Entities;

namespace Tenon.Repository.EfCore.Tests.Interceptors;

/// <summary>
/// 评论软删除拦截器
/// </summary>
public class CommentSoftDeleteInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            HandleCommentSoftDelete(eventData.Context);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            HandleCommentSoftDelete(eventData.Context);
        }
        return base.SavingChanges(eventData, result);
    }

    private void HandleCommentSoftDelete(DbContext context)
    {
        var entries = context.ChangeTracker.Entries<BlogComment>()
            .Where(e => e.State != EntityState.Deleted && e.Entity.IsDeleted)
            .ToList();

        foreach (var entry in entries)
        {
            var childComments = context.Set<BlogComment>()
                .IgnoreQueryFilters()
                .Where(c => c.ParentId == entry.Entity.Id)
                .ToList();

            foreach (var childComment in childComments)
            {
                childComment.IsDeleted = true;
                childComment.DeletedAt = DateTimeOffset.UtcNow;
                childComment.DeletedBy = entry.Entity.DeletedBy;
            }
        }
    }
} 