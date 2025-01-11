using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    /// <param name="configuration">Hangfire 配置节</param>
    /// <param name="setupAction">配置 Hangfire 选项的委托</param>
    /// <param name="configureStorage">配置 Hangfire 存储的委托</param>
    /// <param name="configureServer">配置 Hangfire 服务器选项的委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddHangfireServices(
        this IServiceCollection services,
        IConfigurationSection configuration,
        Action<HangfireOptions>? setupAction = null,
        Action<IGlobalConfiguration>? configureStorage = null,
        Action<BackgroundJobServerOptions>? configureServer = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        // 注册 Hangfire 配置
        services.Configure<HangfireOptions>(configuration);
        
        var hangfireOptions = configuration.Get<HangfireOptions>();
        if (hangfireOptions == null)
        {
            throw new InvalidOperationException($"无法从配置节点 '{configuration.Path}' 中读取 Hangfire 选项");
        }

        // 验证 IP 授权配置
        if (hangfireOptions.IpAuthorization.Enabled && !hangfireOptions.IpAuthorization.IsValid())
        {
            throw new InvalidOperationException("IP 授权配置无效：未配置允许的 IP 地址或 IP 范围");
        }

        if (setupAction != null)
        {
            setupAction(hangfireOptions);
            services.Configure(setupAction);
        }

        // 注册服务
        services.AddSingleton<IPasswordValidator, PasswordValidator>();
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
            // 默认配置
            options.WorkerCount = Environment.ProcessorCount * 2; // 工作线程数
            options.Queues = new[] {"default", "critical"}; // 任务队列
            options.ServerTimeout = TimeSpan.FromMinutes(5); // 服务器超时
            options.ShutdownTimeout = TimeSpan.FromMinutes(2); // 关闭超时
            options.ServerName = $"Hangfire.Server.{Environment.MachineName}"; // 服务器名称

            // 允许用户自定义配置
            configureServer?.Invoke(options);
        });

        return services;
    }

    /// <summary>
    ///     使用 Hangfire
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    public static void UseHangfire(this WebApplication app)
    {
        var serviceProvider = app.Services;
        var logger = serviceProvider.GetRequiredService<ILogger<HangfireService>>();
        var hangfireOptions = serviceProvider.GetRequiredService<IOptions<HangfireOptions>>().Value;

        // 验证必要的服务是否已注册
        var cacheProvider = serviceProvider.GetService<IHangfireCacheProvider>();
        if (cacheProvider == null)
            throw new InvalidOperationException(
                "未找到 IHangfireCacheProvider 的实例。请确保在启动时正确注册了缓存提供程序。");

        var passwordValidator = serviceProvider.GetRequiredService<IPasswordValidator>();
        var loginAttemptTracker = serviceProvider.GetRequiredService<ILoginAttemptTracker>();

        // 准备授权过滤器列表
        var authFilters = new List<IDashboardAuthorizationFilter>();

        // 添加 IP 授权过滤器（如果启用）
        if (hangfireOptions.IpAuthorization.Enabled)
        {
            logger.LogInformation("IP 授权已启用");
            
            if (!hangfireOptions.IpAuthorization.IsValid())
            {
                logger.LogWarning("IP 授权配置无效：未配置允许的 IP 地址或 IP 范围");
                throw new InvalidOperationException("IP 授权配置无效：未配置允许的 IP 地址或 IP 范围");
            }

            logger.LogInformation("允许的 IP: {AllowedIPs}", string.Join(", ", hangfireOptions.IpAuthorization.AllowedIPs));
            logger.LogInformation("允许的 IP 范围: {AllowedIpRanges}", string.Join(", ", hangfireOptions.IpAuthorization.AllowedIpRanges));
            logger.LogInformation("IP 验证通过时是否跳过基本认证: {SkipBasicAuth}", hangfireOptions.SkipBasicAuthenticationIfIpAuthorized);

            var ipFilter = new HangfireIpAuthorizationFilter(
                hangfireOptions.IpAuthorization,
                app.Services.GetRequiredService<ILogger<HangfireIpAuthorizationFilter>>(),
                hangfireOptions.SkipBasicAuthenticationIfIpAuthorized);
            authFilters.Add(ipFilter);
        }
        else
        {
            logger.LogWarning("IP 授权未启用");
        }

        // 添加基本认证过滤器
        var basicAuthFilter = new HangfireBasicAuthenticationFilter(
            loginAttemptTracker,
            passwordValidator,
            hangfireOptions.Authentication,
            app.Services.GetRequiredService<ILogger<HangfireBasicAuthenticationFilter>>());
        authFilters.Add(basicAuthFilter);

        // 根据环境配置防伪令牌设置
        var ignoreAntiforgeryToken = app.Environment.IsDevelopment() || hangfireOptions.IgnoreAntiforgeryToken;
        logger.LogInformation("防伪令牌验证: {Status}", ignoreAntiforgeryToken ? "已禁用" : "已启用");

        var dashboardOptions = new DashboardOptions
        {
            Authorization = authFilters,
            DashboardTitle = hangfireOptions.DashboardTitle,
            IgnoreAntiforgeryToken = ignoreAntiforgeryToken,
            DisplayStorageConnectionString = false
        };

        app.UseHangfireDashboard(hangfireOptions.Path, dashboardOptions);
        logger.LogInformation("Hangfire 仪表板已配置在路径: {Path}", hangfireOptions.Path);
    }
}