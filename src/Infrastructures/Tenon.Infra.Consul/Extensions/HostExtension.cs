using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tenon.Abstractions;

namespace Tenon.Infra.Consul.Extensions;

public static class HostExtension
{
    public static IHost UseConsulRegistrationCenter(this IHost host, Func<Uri> getServiceAddressHandle)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));
        if (getServiceAddressHandle == null)
            throw new ArgumentNullException(nameof(getServiceAddressHandle));
        var serviceInfo = host.Services.GetRequiredService<IWebApiServiceDescriptor>();
        var registration = ActivatorUtilities.CreateInstance<RegistrationProvider>(host.Services);
        registration.Register(getServiceAddressHandle, serviceInfo.Id);
        return host;
    }
}