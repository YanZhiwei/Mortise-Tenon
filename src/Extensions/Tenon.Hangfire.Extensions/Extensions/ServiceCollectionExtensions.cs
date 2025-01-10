using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    public static void AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 注册配置
        services.Configure<HangfireOptions>(configuration.GetSection("Hangfire"));

        // 注册服务
        services.AddMemoryCache();
        services.AddSingleton<IPasswordValidator, PasswordValidator>();
        services.AddSingleton<ILoginAttemptTracker, LoginAttemptTracker>();
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
        var passwordValidator = serviceProvider.GetRequiredService<IPasswordValidator>();
        var loginAttemptTracker = serviceProvider.GetRequiredService<ILoginAttemptTracker>();

        app.UseHangfireDashboard(hangfireOptions.Path, new DashboardOptions
        {
            Authorization =
            [
                new HangfireBasicAuthenticationFilter(
                    hangfireOptions,
                    passwordValidator,
                    loginAttemptTracker,
                    app.Services.GetRequiredService<ILogger<HangfireBasicAuthenticationFilter>>()),
                new HangfireIpAuthorizationFilter(
                    hangfireOptions,
                    app.Services.GetRequiredService<ILogger<HangfireIpAuthorizationFilter>>())
            ],
            DashboardTitle = hangfireOptions.DashboardTitle,
            IgnoreAntiforgeryToken = true, // 在开发环境可以禁用，生产环境建议启用
            DisplayStorageConnectionString = false // 隐藏连接字符串
        });
    }
}