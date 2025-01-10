using System.Text;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Tenon.Hangfire.Extensions.Configuration;
using Tenon.Hangfire.Extensions.Services;

namespace Tenon.Hangfire.Extensions.Filters;

/// <summary>
///     Hangfire 仪表板基本认证过滤器
/// </summary>
public class HangfireBasicAuthenticationFilter : IDashboardAuthorizationFilter
{
    private readonly IPasswordValidator _passwordValidator;
    private readonly ILoginAttemptTracker _loginAttemptTracker;
    private readonly ILogger<HangfireBasicAuthenticationFilter> _logger;
    private readonly HangfireOptions _options;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="options">Hangfire 配置选项</param>
    /// <param name="passwordValidator">密码验证器</param>
    /// <param name="loginAttemptTracker">登录尝试跟踪器</param>
    /// <param name="logger">日志记录器</param>
    public HangfireBasicAuthenticationFilter(
        HangfireOptions options,
        IPasswordValidator passwordValidator,
        ILoginAttemptTracker loginAttemptTracker,
        ILogger<HangfireBasicAuthenticationFilter> logger)
    {
        _options = options;
        _passwordValidator = passwordValidator;
        _loginAttemptTracker = loginAttemptTracker;
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

        // 检查认证头
        var header = httpContext.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Basic "))
        {
            return Challenge(httpContext, "需要认证");
        }

        // 解析认证信息
        var authValues = Encoding.UTF8.GetString(
            Convert.FromBase64String(header["Basic ".Length..])).Split(':');

        if (authValues.Length < 2)
        {
            return Challenge(httpContext, "认证信息格式错误");
        }

        var username = authValues[0];
        var password = authValues[1];

        // 检查用户名
        if (username != _options.Username)
        {
            _logger.LogWarning("用户名错误: {Username}", username);
            return Challenge(httpContext, "用户名或密码错误");
        }

        // 检查账户锁定状态
        if (_loginAttemptTracker.IsAccountLocked(username))
        {
            _logger.LogWarning("账户已锁定: {Username}", username);
            return Challenge(httpContext, $"账户已锁定，请 {_options.Authentication.LockoutDuration} 分钟后重试");
        }

        // 验证密码
        if (_options.Authentication.EnablePasswordComplexity)
        {
            var validationResult = _passwordValidator.ValidatePassword(password, _options.Authentication);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("密码不符合复杂度要求: {Username}", username);
                return Challenge(httpContext, string.Join(", ", validationResult.Errors));
            }
        }

        // 验证密码是否正确
        if (password != _options.Password)
        {
            _loginAttemptTracker.RecordFailedAttempt(username);
            var remainingAttempts = _loginAttemptTracker.GetRemainingAttempts(username);
            _logger.LogWarning("密码错误: {Username}, 剩余尝试次数: {RemainingAttempts}", username, remainingAttempts);
            return Challenge(httpContext, $"用户名或密码错误，剩余尝试次数: {remainingAttempts}");
        }

        // 认证成功，重置失败计数
        _loginAttemptTracker.ResetFailedAttempts(username);
        return true;
    }

    /// <summary>
    ///     发起认证挑战
    /// </summary>
    /// <param name="context">HTTP上下文</param>
    /// <param name="message">错误消息</param>
    /// <returns>始终返回false，触发认证</returns>
    private bool Challenge(HttpContext context, string message)
    {
        context.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "text/plain";
        context.Response.WriteAsync(message);
        return false;
    }
}