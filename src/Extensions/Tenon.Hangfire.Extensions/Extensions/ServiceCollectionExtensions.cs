using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tenon.Caching.Abstractions;
using Tenon.Hangfire.Extensions.Caching;
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
    /// <param name="configureStorage">配置 Hangfire 存储的委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddHangfireServices(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<HangfireOptions>? setupAction = null,
        Action<IGlobalConfiguration>? configureStorage = null)
    {
        // 注册 Hangfire 配置
        var hangfireSection = configuration.GetSection("Hangfire");
        if (setupAction != null)
            services.Configure(setupAction);
        else
            services.Configure<HangfireOptions>(hangfireSection);

        // 配置认证选项
        var authSection = hangfireSection.GetSection("Authentication");
        services.Configure<AuthenticationOptions>(authSection);

        // 注册服务
        services.AddSingleton<IPasswordValidator>(sp => 
            new PasswordValidator(sp.GetRequiredService<IHangfireCacheProvider>()));
        services.AddSingleton<ILoginAttemptTracker>(sp => 
            new LoginAttemptTracker(sp.GetRequiredService<IHangfireCacheProvider>()));

        // 添加 Hangfire 服务
        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();

            // 允许用户配置存储
            configureStorage?.Invoke(config);
        });

        // 配置 Hangfire 服务器选项
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = Environment.ProcessorCount * 2; // 工作线程数
            options.Queues = new[] { "default", "critical" }; // 任务队列
            options.ServerTimeout = TimeSpan.FromMinutes(5); // 服务器超时
            options.ShutdownTimeout = TimeSpan.FromMinutes(2); // 关闭超时
            options.ServerName = $"Hangfire.Server.{Environment.MachineName}"; // 服务器名称
        });

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
        var cacheProvider = serviceProvider.GetService<IHangfireCacheProvider>();
        if (cacheProvider == null)
        {
            throw new InvalidOperationException(
                "未找到 IHangfireCacheProvider 的实例。请确保在启动时正确注册了缓存提供程序。");
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