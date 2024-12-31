using System.Reflection;
using Tenon.Abstractions;

namespace Tenon.AspNetCore;

public class WebServiceDescriptor
    : IWebApiServiceDescriptor
{
    public string Id { get; private init; }
    public string ServiceName { get; private init; }
    public string Version { get; private init; }
    public string Description { get; private init; }
    public string CorsPolicy { get; private init; }

    public static IWebApiServiceDescriptor CreateInstance(Assembly startAssembly, string? serviceName = null,
        string? serviceInternalId = null,
        string corsPolicy = "default")
    {
        var attribute = startAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
        var description = attribute is null ? string.Empty : attribute.Description;
        var version = startAssembly.GetName().Version ??
                      throw new NullReferenceException("startAssembly.GetName().Version");
        if (string.IsNullOrWhiteSpace(serviceName))
            serviceName = startAssembly.GetName().Name ?? string.Empty;

        if (string.IsNullOrWhiteSpace(serviceInternalId))
            serviceInternalId = Convert.ToString(DateTime.UtcNow.Ticks, 16);
        var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower();

        var serviceId = envName switch
        {
            "development" => $"{serviceName}-dev-{serviceInternalId}",
            "test" => $"{serviceName}-test-{serviceInternalId}",
            "staging" => $"{serviceName}-stag-{serviceInternalId}",
            "production" => $"{serviceName}-prod-{serviceInternalId}",
            _ => $"{serviceName}-{envName}-{serviceInternalId}"
        };

        return new WebServiceDescriptor
        {
            Id = serviceId,
            ServiceName = serviceName,
            Version = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}",
            Description = description,
            CorsPolicy = corsPolicy
        };
    }
}