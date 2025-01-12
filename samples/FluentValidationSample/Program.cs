using System.Globalization;
using FluentValidation;
using FluentValidationSample.Models;
using FluentValidationSample.Services;
using FluentValidationSample.Validators;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Tenon.AspNetCore.OpenApi.Extensions;
using Tenon.FluentValidation.AspNetCore.Extensions;
using Tenon.FluentValidation.AspNetCore.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add localization
builder.Services.AddLocalization();

// Configure supported cultures from configuration
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var localizationConfig = builder.Configuration.GetSection("Localization").Get<LocalizationConfig>();
    
    var supportedCultures = localizationConfig?.SupportedCultures?
        .Select(c => new CultureInfo(c))
        .ToArray() ?? new[] { new CultureInfo("zh-CN") };

    var defaultCulture = localizationConfig?.DefaultCulture ?? "zh-CN";

    options.DefaultRequestCulture = new RequestCulture(defaultCulture);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    
    // 配置请求本地化选项
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new QueryStringRequestCultureProvider(),
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});

// Add FluentValidation
builder.Services.AddWebApiFluentValidation(
    builder.Configuration.GetSection("FluentValidation"),
    typeof(Program).Assembly);

// 显式注册验证器
builder.Services.AddScoped<IValidator<UserRegistrationRequest>, UserRegistrationValidator>();

// Add Scalar UI
builder.Services.AddScalarOpenApi(builder.Configuration.GetSection("ScalarUI"));

// 注册 UserService
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.UseScalarOpenApi();

app.UseRequestLocalization();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>
/// 本地化配置
/// </summary>
public class LocalizationConfig
{
    /// <summary>
    /// 默认文化信息
    /// </summary>
    public string DefaultCulture { get; set; } = "zh-CN";

    /// <summary>
    /// 支持的文化信息列表
    /// </summary>
    public string[] SupportedCultures { get; set; } = { "zh-CN" };
}