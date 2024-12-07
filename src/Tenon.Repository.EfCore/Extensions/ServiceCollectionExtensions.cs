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
        services.AddScoped(typeof(IEfRepository<,>), typeof(EfRepository<>));
    }
}
