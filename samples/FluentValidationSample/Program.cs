using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Tenon.AspNetCore.OpenApi.Extensions;
using Tenon.FluentValidation.AspNetCore.Extensions;
using Tenon.FluentValidation.AspNetCore.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add FluentValidation
builder.Services.AddWebApiFluentValidation(builder.Configuration.GetSection("FluentValidation"));

// Add localization
var localizationOptions = builder.Configuration.GetSection("FluentValidation:Localization").Get<LocalizationOptions>();
builder.Services.AddLocalization(options => options.ResourcesPath = localizationOptions?.ResourcesPath ?? "Resources");

// Add Scalar UI
builder.Services.AddScalarOpenApi(builder.Configuration.GetSection("ScalarUI"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.UseScalarOpenApi();

// Configure localization
if (localizationOptions != null)
{
    var supportedCultures = (localizationOptions.SupportedCultures ?? new[] { "zh-CN" })
        .Select(c => new CultureInfo(c))
        .ToList();

    app.UseRequestLocalization(new RequestLocalizationOptions
    {
        DefaultRequestCulture = new RequestCulture(localizationOptions.DefaultCulture ?? "zh-CN"),
        SupportedCultures = supportedCultures,
        SupportedUICultures = supportedCultures
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();