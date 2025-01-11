using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;
using Tenon.AspNetCore.OpenApi.Extensions.Options;

namespace Tenon.AspNetCore.OpenApi.Extensions;

/// <summary>
/// OpenAPI 服务扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 OpenAPI 服务（使用 Scalar UI）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置选项</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddScalarOpenApi(this IServiceCollection services, Action<TenonScalarOptions>? configure = null)
    {
        var options = new TenonScalarOptions();
        configure?.Invoke(options);

        services.AddEndpointsApiExplorer();
        services.AddOpenApi();

        return services;
    }

    /// <summary>
    /// 使用 OpenAPI UI（Scalar）
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <param name="configure">配置选项</param>
    /// <returns>应用程序构建器</returns>
    public static IEndpointRouteBuilder UseScalarOpenApi(this IEndpointRouteBuilder app, Action<TenonScalarOptions>? configure = null)
    {
        var options = new TenonScalarOptions();
        configure?.Invoke(options);

        app.MapOpenApi();
        app.MapScalarApiReference(config =>
        {
            config.Title = options.Title;
            config.DarkMode = options.Theme.DarkMode;

            if (options.EnableAuthorization && options.OAuth2 != null)
            {
                config.Authentication = new ScalarAuthenticationOptions
                {
                    OAuth2 = new()
                    {
                        ClientId = options.OAuth2.ClientId,
                        Scopes = options.OAuth2.Scopes
                    }
                };
            }
        });

        return app;
    }
} 