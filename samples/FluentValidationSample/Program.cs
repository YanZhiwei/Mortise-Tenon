using FluentValidationSample.Services;
using FluentValidationSample.Validators;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Tenon.AspNetCore.OpenApi.Extensions;
using Tenon.FluentValidation.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 获取 FluentValidation 配置
var fluentValidationConfig = builder.Configuration.GetSection("FluentValidation");
var localizationConfig = fluentValidationConfig.GetSection("Localization");

// 添加本地化支持
builder.Services.AddLocalization(options => 
    options.ResourcesPath = localizationConfig.GetValue<string>("ResourcesPath") ?? "Resources");

// 添加 WebAPI FluentValidation 验证器
builder.Services.AddWebApiFluentValidation(fluentValidationConfig, typeof(UserRegistrationValidator).Assembly);

// 添加业务服务
builder.Services.AddScoped<IUserService, UserService>();

// 配置 Scalar OpenAPI
builder.Services.AddScalarOpenApi(builder.Configuration.GetSection("ScalarUI"));

var app = builder.Build();

// 配置本地化
var defaultCulture = localizationConfig.GetValue<string>("DefaultCulture") ?? "zh-CN";
var supportedCultureNames = localizationConfig.GetSection("SupportedCultures").Get<string[]>() 
    ?? new[] { "zh-CN", "en-US" };
var supportedCultures = supportedCultureNames.Select(x => new CultureInfo(x)).ToArray();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.UseScalarOpenApi();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();