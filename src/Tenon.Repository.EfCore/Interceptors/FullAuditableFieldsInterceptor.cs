using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections;

namespace Tenon.Repository.EfCore.Interceptors;

/// <summary>
/// 完整审计字段拦截器
/// </summary>
public class FullAuditableFieldsInterceptor(EfAuditableUser userContext) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            HandleAuditableFields(eventData.Context);
            HandleSoftDelete(eventData.Context);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            HandleAuditableFields(eventData.Context);
            HandleSoftDelete(eventData.Context);
        }
        return base.SavingChanges(eventData, result);
    }

    private void HandleAuditableFields(DbContext context)
    {
        var userId = userContext.User;
        var entries = context.ChangeTracker.Entries<EfFullAuditableEntity>().ToList();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                    entry.Entity.CreatedBy = userId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                    entry.Entity.UpdatedBy = userId;
                    break;
            }
        }
    }

    private void HandleSoftDelete(DbContext context)
    {
        var userId = userContext.User;
        var entries = context.ChangeTracker.Entries<EfFullAuditableEntity>()
            .Where(e => e.State != EntityState.Deleted && e.Entity.IsDeleted)
            .ToList();

        foreach (var entry in entries)
        {
            // 设置软删除字段
            if (entry.Entity.IsDeleted && !entry.Entity.DeletedAt.HasValue)
            {
                entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
                entry.Entity.DeletedBy = userId;

                // 获取所有导航属性
                var navigations = entry.Metadata.GetNavigations()
                    .Where(n => typeof(ISoftDelete).IsAssignableFrom(n.TargetEntityType.ClrType))
                    .ToList();

                foreach (var navigation in navigations)
                {
                    // 获取导航属性的值
                    var navigationValue = entry.Navigation(navigation).CurrentValue;
                    if (navigationValue == null) continue;

                    // 处理集合导航属性
                    if (navigation.IsCollection)
                    {
                        var items = ((IEnumerable)navigationValue).Cast<EfFullAuditableEntity>();
                        foreach (var item in items)
                        {
                            if (!item.IsDeleted)
                            {
                                item.IsDeleted = true;
                                item.DeletedAt = DateTimeOffset.UtcNow;
                                item.DeletedBy = userId;
                            }
                        }
                    }
                    // 处理单个导航属性
                    else
                    {
                        var item = (EfFullAuditableEntity)navigationValue;
                        if (!item.IsDeleted)
                        {
                            item.IsDeleted = true;
                            item.DeletedAt = DateTimeOffset.UtcNow;
                            item.DeletedBy = userId;
                        }
                    }
                }
            }
        }
    }
}