using System.Text;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Tenon.Hangfire.Extensions.Configuration;
using Tenon.Hangfire.Extensions.Services;

namespace Tenon.Hangfire.Extensions.Filters;

/// <summary>
///     Hangfire 基本认证过滤器
/// </summary>
public sealed class HangfireBasicAuthenticationFilter : IDashboardAuthorizationFilter
{
    private readonly ILogger<HangfireBasicAuthenticationFilter> _logger;
    private readonly ILoginAttemptTracker _loginAttemptTracker;
    private readonly AuthenticationOptions _options;
    private readonly IPasswordValidator _passwordValidator;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="loginAttemptTracker">登录尝试跟踪器</param>
    /// <param name="passwordValidator">密码验证器</param>
    /// <param name="options">认证选项</param>
    /// <param name="logger">日志记录器</param>
    public HangfireBasicAuthenticationFilter(
        ILoginAttemptTracker loginAttemptTracker,
        IPasswordValidator passwordValidator,
        AuthenticationOptions options,
        ILogger<HangfireBasicAuthenticationFilter> logger)
    {
        _loginAttemptTracker = loginAttemptTracker;
        _passwordValidator = passwordValidator;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    ///     授权验证
    /// </summary>
    /// <param name="context">仪表板上下文</param>
    /// <returns>是否授权通过</returns>
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // 检查是否需要跳过基本认证
        if (httpContext.Items.TryGetValue("SkipBasicAuth", out var skipAuth) && skipAuth is true)
        {
            _logger.LogDebug("IP 验证已通过，跳过基本认证");
            return true;
        }

        _logger.LogDebug("开始基本认证验证");

        var header = httpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(header) || !header.StartsWith("Basic "))
        {
            SetUnauthorizedResponse(httpContext);
            return false;
        }

        var credentials = GetCredentialsFromHeader(header);
        if (credentials == null)
        {
            SetUnauthorizedResponse(httpContext);
            return false;
        }

        var (username, password) = credentials.Value;

        // 验证用户名
        if (username != _options.Username)
        {
            _logger.LogWarning("用户名不正确: {Username}", username);
            SetUnauthorizedResponse(httpContext);
            return false;
        }

        // 检查账户是否被锁定
        if (_loginAttemptTracker.IsLockedOut(username, _options))
        {
            _logger.LogWarning("账户已被锁定: {Username}", username);
            SetUnauthorizedResponse(httpContext, "账户已被锁定，请稍后重试");
            return false;
        }

        // 验证密码
        if (!_passwordValidator.ValidatePassword(password, _options))
        {
            _logger.LogWarning("密码验证失败: {Username}", username);
            _loginAttemptTracker.RecordFailedAttempt(username, _options);
            SetUnauthorizedResponse(httpContext, "用户名或密码错误");
            return false;
        }

        // 验证通过，重置失败次数
        _loginAttemptTracker.ResetAttempts(username);
        _logger.LogDebug("认证成功: {Username}", username);
        return true;
    }

    private static (string username, string password)? GetCredentialsFromHeader(string header)
    {
        try
        {
            var credentials = Encoding.UTF8.GetString(
                Convert.FromBase64String(header.Substring(6)));
            var parts = credentials.Split(':');
            if (parts.Length != 2)
                return null;

            return (parts[0], parts[1]);
        }
        catch
        {
            return null;
        }
    }

    private static void SetUnauthorizedResponse(HttpContext context, string message = "")
    {
        context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\"";
        context.Response.StatusCode = 401;
        if (!string.IsNullOrEmpty(message))
        {
            context.Response.ContentType = "text/plain";
            context.Response.WriteAsync(message).GetAwaiter().GetResult();
        }
    }
}