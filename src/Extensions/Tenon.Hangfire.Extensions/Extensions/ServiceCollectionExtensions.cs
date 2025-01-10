using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tenon.Caching.Abstractions;
using Tenon.Hangfire.Extensions.Configuration;
using Tenon.Hangfire.Extensions.Filters;
using Tenon.Hangfire.Extensions.Services;

namespace Tenon.Hangfire.Extensions.Extensions;

/// <summary>
///     服务集合扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     添加 Hangfire 服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <param name="setupAction">配置 Hangfire 选项的委托</param>
    /// <param name="configureCache">配置缓存提供程序的委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddHangfireServices(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<HangfireOptions>? setupAction = null,
        Action<IServiceCollection>? configureCache = null)
    {
        // 注册配置
        if (setupAction != null)
            services.Configure(setupAction);
        else
            services.Configure<HangfireOptions>(configuration.GetSection("Hangfire"));

        // 注册缓存服务
        if (configureCache != null)
        {
            configureCache(services);
        }
        else
        {
            // 确保已注册 ICacheProvider
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICacheProvider));
            if (descriptor == null)
            {
                throw new InvalidOperationException(
                    "未找到 ICacheProvider 的注册。请在调用 AddHangfireServices 之前注册缓存提供程序，" +
                    "或使用 configureCache 参数配置缓存服务。");
            }
        }

        // 注册服务
        services.AddSingleton<IPasswordValidator, PasswordValidator>();
        services.AddSingleton<ILoginAttemptTracker, LoginAttemptTracker>();

        return services;
    }

    /// <summary>
    ///     使用 Hangfire
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <param name="hangfireSection">Hangfire 配置节</param>
    public static void UseHangfire(this WebApplication app, IConfigurationSection hangfireSection)
    {
        if (hangfireSection == null)
            throw new ArgumentNullException(nameof(hangfireSection));

        var hangfireOptions = hangfireSection.Get<HangfireOptions>();
        if (hangfireOptions == null)
            throw new ArgumentNullException(nameof(hangfireOptions));

        var serviceProvider = app.Services;
        
        // 验证必要的服务是否已注册
        var cacheProvider = serviceProvider.GetService<ICacheProvider>();
        if (cacheProvider == null)
        {
            throw new InvalidOperationException(
                "未找到 ICacheProvider 的实例。请确保在启动时正确注册了缓存提供程序。");
        }

        var passwordValidator = serviceProvider.GetRequiredService<IPasswordValidator>();
        var loginAttemptTracker = serviceProvider.GetRequiredService<ILoginAttemptTracker>();

        app.UseHangfireDashboard(hangfireOptions.Path, new DashboardOptions
        {
            Authorization =
            [
                new HangfireBasicAuthenticationFilter(
                    loginAttemptTracker,
                    passwordValidator,
                    hangfireOptions.Authentication,
                    app.Services.GetRequiredService<ILogger<HangfireBasicAuthenticationFilter>>()),
                new HangfireIpAuthorizationFilter(
                    hangfireOptions.IpAuthorization,
                    app.Services.GetRequiredService<ILogger<HangfireIpAuthorizationFilter>>())
            ],
            DashboardTitle = hangfireOptions.DashboardTitle,
            IgnoreAntiforgeryToken = true, // 在开发环境可以禁用，生产环境建议启用
            DisplayStorageConnectionString = false // 隐藏连接字符串
        });
    }
}