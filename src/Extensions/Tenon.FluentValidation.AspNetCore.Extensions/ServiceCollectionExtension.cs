﻿using System.ComponentModel.DataAnnotations;
using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tenon.FluentValidation.AspNetCore.Extensions.Interceptors;

namespace Tenon.FluentValidation.AspNetCore.Extensions;

/// <summary>
/// FluentValidation 配置选项
/// </summary>
public class FluentValidationOptions
{
    /// <summary>
    /// 是否禁用 ASP.NET Core 默认的模型验证响应
    /// </summary>
    [Required(ErrorMessage = "必须指定是否禁用默认模型验证响应")]
    public bool DisableDefaultModelValidation { get; set; } = true;

    /// <summary>
    /// 验证器生命周期
    /// </summary>
    [Required(ErrorMessage = "必须指定验证器生命周期")]
    public ServiceLifetime ValidatorLifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>
    /// 是否启用验证消息本地化
    /// </summary>
    [Required(ErrorMessage = "必须指定是否启用本地化")]
    public bool EnableLocalization { get; set; } = false;

    /// <summary>
    /// 验证拦截器类型集合
    /// </summary>
    public List<Type> InterceptorTypes { get; set; } = new();
}

/// <summary>
/// FluentValidation 服务扩展
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    /// 为 ASP.NET Core WebAPI 添加 FluentValidation 验证器
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="assemblies">包含验证器的程序集</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddWebApiFluentValidation(this IServiceCollection services,
        params Assembly[] assemblies)
    {
        return services.AddWebApiFluentValidation(options => { }, assemblies);
    }

    /// <summary>
    /// 为 ASP.NET Core WebAPI 添加 FluentValidation 验证器
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置节</param>
    /// <param name="assemblies">包含验证器的程序集</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddWebApiFluentValidation(this IServiceCollection services,
        IConfigurationSection configuration,
        params Assembly[] assemblies)
    {
        services.AddOptions<FluentValidationOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return AddWebApiFluentValidationCore(services, assemblies);
    }

    /// <summary>
    /// 为 ASP.NET Core WebAPI 添加 FluentValidation 验证器
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOptions">配置选项</param>
    /// <param name="assemblies">包含验证器的程序集</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddWebApiFluentValidation(this IServiceCollection services,
        Action<FluentValidationOptions> configureOptions,
        params Assembly[] assemblies)
    {
        services.AddOptions<FluentValidationOptions>()
            .Configure(configureOptions)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return AddWebApiFluentValidationCore(services, assemblies);
    }

    private static IServiceCollection AddWebApiFluentValidationCore(IServiceCollection services, Assembly[] assemblies)
    {
        var options = services.BuildServiceProvider()
            .GetRequiredService<IOptions<FluentValidationOptions>>()
            .Value;

        // 注册验证器
        services.AddValidatorsFromAssemblies(assemblies, options.ValidatorLifetime);

        // 配置 API 行为选项
        if (options.DisableDefaultModelValidation)
        {
            services.Configure<ApiBehaviorOptions>(opt =>
            {
                opt.SuppressModelStateInvalidFilter = true;
            });
        }

        // 启用本地化支持
        if (options.EnableLocalization)
        {
            // 自动注册本地化验证拦截器
            if (!options.InterceptorTypes.Contains(typeof(LocalizationValidatorInterceptor)))
            {
                options.InterceptorTypes.Add(typeof(LocalizationValidatorInterceptor));
            }
        }

        // 注册验证拦截器
        foreach (var interceptorType in options.InterceptorTypes)
        {
            services.AddScoped(typeof(IValidatorInterceptor), interceptorType);
        }

        return services;
    }
}