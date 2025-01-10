using Hangfire.Dashboard;

namespace Tenon.Hangfire.Extensions.Filters;

public sealed class HangfireIpAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();
        var allowedIps = new[] { "127.0.0.1", "::1" };
        return allowedIps.Contains(remoteIp);
    }
}