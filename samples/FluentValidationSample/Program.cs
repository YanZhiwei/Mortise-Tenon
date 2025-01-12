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

// Configure supported cultures
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("zh-CN"),
        new CultureInfo("en-US")
    };

    options.DefaultRequestCulture = new RequestCulture("zh-CN");
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

// Add FluentValidation with localization enabled
builder.Services.AddWebApiFluentValidation(options =>
{
    options.EnableLocalization = true;
    options.DisableDefaultModelValidation = false;
}, typeof(Program).Assembly);

// Configure API behavior options
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = false;
    options.InvalidModelStateResponseFactory = context =>
    {
        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Instance = context.HttpContext.Request.Path,
        };

        return new BadRequestObjectResult(problemDetails);
    };
});

// 显式注册验证器
builder.Services.AddScoped<IValidator<UserRegistrationRequest>, UserRegistrationValidator>();

// Add Scalar UI
builder.Services.AddScalarOpenApi(options =>
{
    builder.Configuration.GetSection("ScalarUI").Bind(options);
});

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