using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tenon.Repository.EfCore.Configurations;
using Tenon.Repository.EfCore.Interceptors;
using Tenon.Repository.EfCore.Transaction;

namespace Tenon.Repository.EfCore.Extensions;

/// <summary>
///     EF Core 扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     添加 EF Core 配置
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="dbSection">数据库配置节</param>
    /// <param name="optionsAction">数据库选项配置</param>
    /// <param name="interceptors">拦截器数组</param>
    /// <typeparam name="TDbContext">数据库上下文类型</typeparam>
    public static IServiceCollection AddEfCore<TDbContext>(this IServiceCollection services,
        IConfigurationSection dbSection,
        Action<DbContextOptionsBuilder> optionsAction,
        IInterceptor[]? interceptors = null)
        where TDbContext : DbContext
    {
        var dbConfig = dbSection.Get<DbOptions>();
        if (dbConfig == null)
            throw new ArgumentNullException(nameof(dbConfig));

        services.Configure<DbOptions>(dbSection);
        services.AddDbContext<TDbContext>((serviceProvider, options) =>
        {
            ConfigureInterceptors(serviceProvider, options, interceptors);
            optionsAction(options);
        });

        // 注册 DbContext 作为基类
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TDbContext>());

        // 获取所有实体类型
        var entityTypes = typeof(TDbContext).GetProperties()
            .Where(p => p.PropertyType.IsGenericType && 
                       p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0])
            .ToArray();

        // 移除现有的仓储注册
        services.RemoveAll(typeof(IEfRepository<,>));
        services.RemoveAll(typeof(IRepository<,>));

        // 为每个实体类型注册仓储
        foreach (var entityType in entityTypes)
        {
            // 注册具体的仓储实现
            var repositoryType = typeof(EfRepository<>).MakeGenericType(entityType);
            services.AddScoped(repositoryType);

            // 注册接口映射
            var repositoryInterfaceType = typeof(IRepository<,>).MakeGenericType(entityType, typeof(long));
            var efRepositoryInterfaceType = typeof(IEfRepository<,>).MakeGenericType(entityType, typeof(long));

            services.AddScoped(repositoryInterfaceType, sp => sp.GetRequiredService(repositoryType));
            services.AddScoped(efRepositoryInterfaceType, sp => sp.GetRequiredService(repositoryType));
        }

        // 注册通用服务
        RegisterCommonServices(services);
        return services;
    }

    /// <summary>
    ///     配置拦截器
    /// </summary>
    private static void ConfigureInterceptors(IServiceProvider serviceProvider, DbContextOptionsBuilder options,
        IInterceptor[]? interceptors)
    {
        if (interceptors?.Any() ?? false)
            options.AddInterceptors(interceptors);

        var basicAuditableInterceptor = serviceProvider.GetService<BasicAuditableFieldsInterceptor>();
        if (basicAuditableInterceptor != null)
            options.AddInterceptors(basicAuditableInterceptor);
    }

    /// <summary>
    ///     注册通用服务
    /// </summary>
    private static void RegisterCommonServices(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}
