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

        services.AddScoped<FileUploadValidationFilter>();

        return services;
    }
}