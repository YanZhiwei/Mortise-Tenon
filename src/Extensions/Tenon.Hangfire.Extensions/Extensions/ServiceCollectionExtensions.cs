using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Tenon.Hangfire.Extensions.Configuration;
using Tenon.Hangfire.Extensions.Filters;

namespace Tenon.Hangfire.Extensions.Extensions;

public static class ServiceCollectionExtensions
{
    public static void UseHangfire(this WebApplication app, IConfigurationSection hangfireSection)
    {
        if (hangfireSection == null)
            throw new ArgumentNullException(nameof(hangfireSection));

        var hangfireOptions = hangfireSection.Get<HangfireOptions>();
        if (hangfireOptions == null)
            throw new ArgumentNullException(nameof(hangfireOptions));

        app.UseHangfireDashboard(hangfireOptions.Path, new DashboardOptions
        {
            Authorization =
            [
                new HangfireBasicAuthenticationFilter
                {
                    User = hangfireOptions.Username,
                    Pass = hangfireOptions.Password
                }
            ],
            DashboardTitle = hangfireOptions.DashboardTitle,
            IgnoreAntiforgeryToken = true, // 在开发环境可以禁用，生产环境建议启用
            DisplayStorageConnectionString = false // 隐藏连接字符串
        });
    }
}