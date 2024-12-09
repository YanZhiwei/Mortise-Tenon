using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Tenon.Repository.EfCore.Extensions;

namespace Tenon.Repository.EfCore;

/// <summary>
/// Tenon基础数据库上下文
/// </summary>
public abstract class TenonDbContext : DbContext
{
    protected TenonDbContext(DbContextOptions options) : base(options)
    {
    }

    /// <summary>
    /// 获取实体所在程序集
    /// </summary>
    protected abstract Assembly EntityAssembly { get; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 应用所有实体配置
        modelBuilder.ApplyConfigurationsFromAssembly(EntityAssembly);

        // 为所有实现了ISoftDelete的实体添加全局软删除过滤器
        modelBuilder.ApplySoftDeleteQueryFilter();
    }
} 