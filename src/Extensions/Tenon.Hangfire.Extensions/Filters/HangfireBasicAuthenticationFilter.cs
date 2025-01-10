using System.Text;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace Tenon.Hangfire.Extensions.Filters;

/// <summary>
///     Hangfire 仪表板基本认证过滤器
/// </summary>
public class HangfireBasicAuthenticationFilter : IDashboardAuthorizationFilter
{
    /// <summary>
    ///     密码
    /// </summary>
    public string Pass { get; set; } = string.Empty;

    /// <summary>
    ///     用户名
    /// </summary>
    public string User { get; set; } = string.Empty;

    /// <summary>
    ///     授权验证
    /// </summary>
    /// <param name="context">仪表板上下文</param>
    /// <returns>是否授权通过</returns>
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        var header = httpContext.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Basic ")) return Challenge(httpContext);

        var authValues = Encoding.UTF8.GetString(
            Convert.FromBase64String(header.Substring(6))).Split(':');

        if (authValues.Length < 2) return Challenge(httpContext);

        var username = authValues[0];
        var password = authValues[1];

        if (username == User && password == Pass) return true;

        return Challenge(httpContext);
    }

    /// <summary>
    ///     发起认证挑战
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <returns>始终返回false，触发认证</returns>
    private bool Challenge(HttpContext context)
    {
        context.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");
        context.Response.StatusCode = 401;
        return false;
    }
}