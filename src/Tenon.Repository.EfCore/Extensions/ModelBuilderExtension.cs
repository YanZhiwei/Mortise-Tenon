using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Tenon.Repository;

namespace Tenon.Repository.EfCore.Extensions;

/// <summary>
/// ModelBuilder扩展方法
/// </summary>
public static class ModelBuilderExtension
{
    /// <summary>
    /// 应用软删除查询过滤器
    /// </summary>
    public static void ApplySoftDeleteQueryFilter(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                var falseConstant = Expression.Constant(false);
                var lambda = Expression.Lambda(Expression.Equal(property, falseConstant), parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
    }
}