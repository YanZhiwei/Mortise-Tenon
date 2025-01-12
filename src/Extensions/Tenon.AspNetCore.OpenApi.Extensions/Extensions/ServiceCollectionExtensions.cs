using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Tenon.AspNetCore.OpenApi.Extensions.Configurations;
using Tenon.AspNetCore.OpenApi.Extensions.ModelBinding;
using Tenon.AspNetCore.OpenApi.Extensions.Transformers;

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
    /// <param name="configuration">配置节</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddScalarOpenApi(this IServiceCollection services, IConfigurationSection configuration)
    {
        services.AddOptions<ScalarUIOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return AddScalarOpenApiCore(services);
    }

    /// <summary>
    /// 添加 OpenAPI 服务（使用 Scalar UI）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置选项</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddScalarOpenApi(this IServiceCollection services, Action<ScalarUIOptions>? configure = null)
    {
        if (configure != null)
        {
            services.AddOptions<ScalarUIOptions>()
                .Configure(configure)
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }

        return AddScalarOpenApiCore(services);
    }

    /// <summary>
    /// 添加 OpenAPI 核心服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    private static IServiceCollection AddScalarOpenApiCore(IServiceCollection services)
    {
        services.Configure<JsonOptions>(options =>
        {
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });

        services.AddEndpointsApiExplorer();
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            options.AddDocumentTransformer<CommaDelimitedArrayDocumentTransformer>();
        });

        return services;
    }

    /// <summary>
    /// 使用 OpenAPI UI（Scalar）
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <returns>应用程序构建器</returns>
    public static IEndpointRouteBuilder UseScalarOpenApi(this IEndpointRouteBuilder app)
    {
        var options = app.ServiceProvider.GetRequiredService<IOptions<ScalarUIOptions>>().Value;
        Validator.ValidateObject(options, new ValidationContext(options), validateAllProperties: true);

        app.MapOpenApi();
        app.MapScalarApiReference(config =>
        {
            config.Title = options.Title;
            config.DarkMode = options.Theme.DarkMode;
        });

        return app;
    }
}