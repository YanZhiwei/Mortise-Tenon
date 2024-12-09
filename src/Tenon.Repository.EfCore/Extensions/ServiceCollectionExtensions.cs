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
///     EF Core 扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     添加 EF Core 配置
    /// </summary>
    /// <typeparam name="TDbContext">数据库上下文类型</typeparam>
    /// <typeparam name="TUnitOfWork">工作单元类型</typeparam>
    public static IServiceCollection AddEfCore<TDbContext, TUnitOfWork>(
        this IServiceCollection services,
        IConfigurationSection dbSection,
        Action<DbContextOptionsBuilder> optionsAction)
        where TDbContext : DbContext
        where TUnitOfWork : class, IUnitOfWork
    {
        var dbConfig = dbSection.Get<DbOptions>();
        if (dbConfig == null)
            throw new ArgumentNullException(nameof(dbConfig));

        services.Configure<DbOptions>(dbSection);

        services.AddDbContext<TDbContext>((serviceProvider, options) =>
        {
            ConfigureInterceptors(serviceProvider, options);
            optionsAction(options);
        });

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TDbContext>());

        RegisterRepositories(services, typeof(TDbContext).Assembly);

        services.AddScoped<IUnitOfWork, TUnitOfWork>();

        return services;
    }

    /// <summary>
    ///     添加 EF Core 配置（使用默认 UnitOfWork）
    /// </summary>
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
    ///     配置拦截器
    /// </summary>
    private static void ConfigureInterceptors(IServiceProvider serviceProvider, DbContextOptionsBuilder options)
    {
        var auditableUser = serviceProvider.GetService<EfAuditableUser>();
        if (auditableUser != null)
        {
            var fullAuditableFieldsInterceptor = new FullAuditableFieldsInterceptor(auditableUser);
            options.AddInterceptors(fullAuditableFieldsInterceptor);
        }

        options.AddInterceptors(new BasicAuditableFieldsInterceptor());
        options.AddInterceptors(new ConcurrencyCheckInterceptor());
    }

    /// <summary>
    ///     获取程序集中的所有实体类型
    /// </summary>
    /// <param name="assembly">程序集</param>
    /// <returns>实体类型集合</returns>
    private static Type[] GetEntityTypes(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(type =>
                type is { IsAbstract: false, IsInterface: false } &&
                Enumerable.Any(type.GetInterfaces(), i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>)))
            .ToArray();
    }

    /// <summary>
    ///     注册所有实体的仓储
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