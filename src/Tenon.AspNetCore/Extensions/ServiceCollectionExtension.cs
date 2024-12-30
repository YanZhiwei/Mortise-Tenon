using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tenon.AspNetCore.Authentication.Bearer;
using Tenon.AspNetCore.Authorization;
using Tenon.AspNetCore.Configuration;
using Tenon.AspNetCore.Filters;
using Tenon.AspNetCore.Localization;

namespace Tenon.AspNetCore.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection ConfigureJwtBearerAuthenticationOptions<TBearerAuthenticationHandler>(
        this IServiceCollection services, IConfigurationSection jwtSection,
        Action<BearerSchemeOptions> configureOptions, string? displayName = null)
        where TBearerAuthenticationHandler : AuthenticationHandler<BearerSchemeOptions>
    {
        if (jwtSection == null)
            throw new ArgumentNullException(nameof(jwtSection));
        var jwtOptions = jwtSection.Get<JwtOptions>();
        if (jwtOptions == null)
            throw new ArgumentNullException(nameof(jwtSection));
        services.Configure<JwtOptions>(jwtSection);
        services.AddAuthentication(BearerDefaults.AuthenticationScheme)
            .AddScheme<BearerSchemeOptions, TBearerAuthenticationHandler>(BearerDefaults.AuthenticationScheme,
                displayName,
                configureOptions);
        return services;
    }

    public static IServiceCollection AddAuthorization<TAuthorizationHandler, TAuthorizationRequirement>(
        this IServiceCollection services, string? policyName = null)
        where TAuthorizationRequirement : IAuthorizationRequirement, new()
        where TAuthorizationHandler : class, IAuthorizationHandler
    {
        if (string.IsNullOrEmpty(policyName))
            policyName = AuthorizePolicy.Default;
        services
            .AddScoped<IAuthorizationHandler, TAuthorizationHandler>();
        return services
            .AddAuthorization(options =>
            {
                options.AddPolicy(policyName,
                    policy => { policy.Requirements.Add(new TAuthorizationRequirement()); });
            });
    }


    public static IServiceCollection AddCors(this IServiceCollection services, string policyName,
        IConfigurationSection corsHostSection)
    {
        if (string.IsNullOrWhiteSpace(policyName))
            throw new ArgumentNullException(nameof(policyName));
        if (corsHostSection == null)
            throw new ArgumentNullException(nameof(corsHostSection));
        var corsHosts = corsHostSection.Get<string>();
        if (string.IsNullOrWhiteSpace(corsHosts))
            throw new ArgumentNullException(nameof(corsHosts));
        Action<CorsPolicyBuilder> corsPolicyAction =
            corsPolicy => corsPolicy.AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        if (corsHosts == "*")
            corsPolicyAction += corsPolicy => corsPolicy.SetIsOriginAllowed(_ => true);
        else
            corsPolicyAction += corsPolicy => corsPolicy.WithOrigins(corsHosts.Split(','));

        return services.AddCors(options => options.AddPolicy(policyName, corsPolicyAction));
    }

    /// <summary>
    /// 添加文件上传服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="fileUploadSection">文件上传配置节</param>
    /// <returns>服务集合</returns>
    /// <exception cref="ArgumentNullException">
    /// 当 <paramref name="fileUploadSection"/> 为 null 时抛出
    /// 或当无法从配置节获取到 <see cref="FileUploadOptions"/> 时抛出
    /// </exception>
    /// <remarks>
    /// 此方法会注册以下服务：
    /// <list type="bullet">
    /// <item><description>文件上传配置选项 (<see cref="FileUploadOptions"/>)</description></item>
    /// <item><description>文件上传验证过滤器 (<see cref="FileUploadValidationFilter"/>)</description></item>
    /// <item><description>本地化服务</description></item>
    /// </list>
    /// 
    /// 使用示例：
    /// <code>
    /// // 在 Program.cs 中配置服务
    /// builder.Services
    ///     .AddLocalization(options => options.ResourcesPath = "Resources")
    ///     .AddFileUpload(builder.Configuration.GetSection("FileUploadSettings"));
    /// 
    /// // 配置支持的语言
    /// var supportedCultures = new[]
    /// {
    ///     new CultureInfo("en"),
    ///     new CultureInfo("zh")
    /// };
    /// 
    /// // 配置请求本地化选项
    /// app.UseRequestLocalization(new RequestLocalizationOptions
    /// {
    ///     DefaultRequestCulture = new RequestCulture("en"),
    ///     SupportedCultures = supportedCultures,
    ///     SupportedUICultures = supportedCultures
    /// });
    /// 
    /// // 在控制器中使用
    /// [HttpPost]
    /// [ServiceFilter(typeof(FileUploadValidationFilter))]
    /// public async Task&lt;IActionResult&gt; Upload(IFormFile file)
    /// {
    ///     // 处理文件上传
    ///     return Ok();
    /// }
    /// </code>
    /// 
    /// appsettings.json 配置示例：
    /// <code>
    /// {
    ///   "FileUploadSettings": {
    ///     "MaxRequestBodySize": 104857600,  // 100MB
    ///     "MaxFileSize": 10485760,          // 10MB
    ///     "AllowedExtensions": [".jpg", ".png", ".pdf"],
    ///     "ValidateFileName": true
    ///   }
    /// }
    /// </code>
    /// </remarks>
    public static IServiceCollection AddFileUpload(this IServiceCollection services, IConfigurationSection fileUploadSection)
    {
        if (fileUploadSection == null)
            throw new ArgumentNullException(nameof(fileUploadSection));
            
        var fileUploadOptions = fileUploadSection.Get<FileUploadOptions>();
        if (fileUploadOptions == null)
            throw new ArgumentNullException(nameof(fileUploadOptions));

        fileUploadOptions.Validate();

        services.Configure<FileUploadOptions>(fileUploadSection);
        services.Configure<FormOptions>(formOptions =>
        {
            formOptions.MultipartBodyLengthLimit = fileUploadOptions.MaxFileSize;
        });

        services.AddLocalization();
        services.AddSingleton<FileValidationLocalizer>();
        services.AddScoped<FileUploadValidationFilter>();

        var serviceProvider = services.BuildServiceProvider();
        var localizer = serviceProvider.GetRequiredService<FileValidationLocalizer>();
        FileValidationResult.Configure(localizer);

        return services;
    }
}