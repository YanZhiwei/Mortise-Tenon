using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tenon.Repository.EfCore.Configurations;
using Tenon.Repository.EfCore.Interceptors;
using Tenon.Repository.EfCore.Transaction;

namespace Tenon.Repository.EfCore.Extensions;

/// <summary>
/// EF Core 扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 EF Core 配置
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文类型</typeparam>
    /// <typeparam name="TUnitOfWork">工作单元类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="dbSection">数据库配置节</param>
    /// <param name="optionsAction">数据库上下文配置委托</param>
    /// <returns>服务集合</returns>
    /// <exception cref="ArgumentNullException">当数据库配置为空时抛出</exception>
    public static IServiceCollection AddEfCore<TDbContext, TUnitOfWork>(
        this IServiceCollection services,
        IConfigurationSection dbSection,
        Action<DbContextOptionsBuilder> optionsAction)
        where TDbContext : DbContext
        where TUnitOfWork : class, IUnitOfWork
    {
        // 获取并验证配置
        var dbOptions = dbSection.Get<DbOptions>();
        if (dbOptions == null)
        {
            throw new ArgumentNullException(nameof(dbOptions), "数据库配置不能为空");
        }
        dbOptions.Validate();

        // 注册配置
        services.Configure<DbOptions>(dbSection);

        // 配置 DbContext
        services.AddDbContext<TDbContext>((serviceProvider, options) =>
        {
            // 配置基础选项
            ConfigureBaseOptions(options, dbOptions);
            
            // 配置拦截器
            ConfigureInterceptors(serviceProvider, options);
            
            // 应用自定义配置
            optionsAction(options);
        });

        // 注册 DbContext
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TDbContext>());

        // 注册仓储
        RegisterRepositories(services, typeof(TDbContext).Assembly);

        // 注册工作单元
        services.AddScoped<IUnitOfWork, TUnitOfWork>();

        return services;
    }

    /// <summary>
    /// 添加 EF Core 配置（使用默认 UnitOfWork）
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="dbSection">数据库配置节</param>
    /// <param name="optionsAction">数据库上下文配置委托</param>
    /// <param name="interceptors">额外的拦截器</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddEfCore<TDbContext>(
        this IServiceCollection services,
        IConfigurationSection dbSection,
        Action<DbContextOptionsBuilder> optionsAction,
        IInterceptor[]? interceptors = null)
        where TDbContext : DbContext
    {
        return AddEfCore<TDbContext, UnitOfWork>(services, dbSection, optionsAction);
    }

    /// <summary>
    /// 配置基础选项
    /// </summary>
    /// <param name="options">数据库上下文配置构建器</param>
    /// <param name="dbOptions">数据库配置选项</param>
    private static void ConfigureBaseOptions(DbContextOptionsBuilder options, DbOptions dbOptions)
    {
        if (options is DbContextOptionsBuilder<DbContext> relationalOptions)
        {
            // 配置命令超时
            if (dbOptions.CommandTimeout > 0)
            {
                relationalOptions.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }
        }

        // 配置详细错误
        if (dbOptions.EnableDetailedErrors)
        {
            options.EnableDetailedErrors();
        }

        // 配置敏感数据日志
        if (dbOptions.EnableSensitiveDataLogging)
        {
            options.EnableSensitiveDataLogging();
        }

        // 配置重试策略
        if (dbOptions.MaxRetryCount > 0)
        {
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }

    /// <summary>
    /// 配置拦截器
    /// </summary>
    /// <param name="serviceProvider">服务提供者</param>
    /// <param name="options">数据库上下文配置构建器</param>
    private static void ConfigureInterceptors(IServiceProvider serviceProvider, DbContextOptionsBuilder options)
    {
        var auditableUser = serviceProvider.GetService<IAuditable<long>>();
        if (auditableUser != null)
        {
            var fullAuditableFieldsInterceptor = new FullAuditableFieldsInterceptor(auditableUser);
            options.AddInterceptors(fullAuditableFieldsInterceptor);
        }

        options.AddInterceptors(new BasicAuditableFieldsInterceptor());
        options.AddInterceptors(new ConcurrencyCheckInterceptor());
    }

    /// <summary>
    /// 获取程序集中的所有实体类型
    /// </summary>
    /// <param name="assembly">程序集</param>
    /// <returns>实体类型集合</returns>
    private static Type[] GetEntityTypes(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(type =>
                type is { IsAbstract: false, IsInterface: false } &&
                type.GetInterfaces().Any(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>)))
            .ToArray();
    }

    /// <summary>
    /// 注册所有实体的仓储
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="assembly">程序集</param>
    private static void RegisterRepositories(IServiceCollection services, Assembly assembly)
    {
        var entityTypes = GetEntityTypes(assembly);

        foreach (var entityType in entityTypes)
        {
            var repositoryType = typeof(EfRepository<>).MakeGenericType(entityType);
            services.AddScoped(repositoryType);

            var repositoryInterfaceType = typeof(IRepository<,>).MakeGenericType(entityType, typeof(long));
            var efRepositoryInterfaceType = typeof(IEfRepository<,>).MakeGenericType(entityType, typeof(long));

            services.AddScoped(repositoryInterfaceType, sp => sp.GetRequiredService(repositoryType));
            services.AddScoped(efRepositoryInterfaceType, sp => sp.GetRequiredService(repositoryType));
        }
    }
}