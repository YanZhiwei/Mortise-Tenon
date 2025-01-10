using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Dashboard;
using Microsoft.Extensions.Logging;
using Tenon.Hangfire.Extensions.Configuration;
using Tenon.Hangfire.Extensions.Services;

namespace Tenon.Hangfire.Extensions.Filters;

/// <summary>
/// Hangfire 基本认证过滤器
/// </summary>
public class HangfireBasicAuthenticationFilter : IDashboardAuthorizationFilter
{
    private readonly ILoginAttemptTracker _loginAttemptTracker;
    private readonly IPasswordValidator _passwordValidator;
    private readonly AuthenticationOptions _options;
    private readonly ILogger<HangfireBasicAuthenticationFilter> _logger;

    public HangfireBasicAuthenticationFilter(
        ILoginAttemptTracker loginAttemptTracker,
        IPasswordValidator passwordValidator,
        AuthenticationOptions options,
        ILogger<HangfireBasicAuthenticationFilter> logger)
    {
        _loginAttemptTracker = loginAttemptTracker ?? throw new ArgumentNullException(nameof(loginAttemptTracker));
        _passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 授权验证
    /// </summary>
    /// <param name="context">仪表板上下文</param>
    /// <returns>是否授权通过</returns>
    public bool Authorize(DashboardContext context)
    {
        try
        {
            var header = context.GetHttpContext().Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(header))
            {
                SetChallengeResponse(context);
                return false;
            }

            var authValues = AuthenticationHeaderValue.Parse(header);
            if (!"Basic".Equals(authValues.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("不支持的认证方案: {Scheme}", authValues.Scheme);
                SetChallengeResponse(context);
                return false;
            }

            var parameter = Encoding.UTF8.GetString(Convert.FromBase64String(authValues.Parameter));
            var parts = parameter.Split(':', 2);
            if (parts.Length < 2)
            {
                _logger.LogWarning("无效的认证凭据格式");
                SetChallengeResponse(context);
                return false;
            }

            var username = parts[0];
            var password = parts[1];

            // 检查账户是否被锁定
            if (_loginAttemptTracker.IsAccountLocked(username))
            {
                _logger.LogWarning("账户已被锁定: {Username}", username);
                context.GetHttpContext().Response.StatusCode = 403;
                return false;
            }

            // 验证凭据
            if (username != _options.Username || password != _options.Password)
            {
                _loginAttemptTracker.RecordFailedAttempt(
                    username,
                    _options.MaxLoginAttempts,
                    TimeSpan.FromMinutes(_options.LockoutDuration));

                var remainingAttempts = _loginAttemptTracker.GetRemainingAttempts(
                    username,
                    _options.MaxLoginAttempts);

                _logger.LogWarning(
                    "认证失败: {Username}, 剩余尝试次数: {RemainingAttempts}",
                    username,
                    remainingAttempts);

                SetChallengeResponse(context);
                return false;
            }

            // 验证密码复杂度
            if (_options.EnablePasswordComplexity)
            {
                var validationResult = _passwordValidator.ValidatePassword(password);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("密码不符合复杂度要求: {Errors}", string.Join(", ", validationResult.Errors));
                    SetChallengeResponse(context);
                    return false;
                }
            }

            // 认证成功，重置失败计数
            _loginAttemptTracker.ResetFailedAttempts(username);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "认证过程发生错误");
            SetChallengeResponse(context);
            return false;
        }
    }

    private void SetChallengeResponse(DashboardContext context)
    {
        var response = context.GetHttpContext().Response;
        response.StatusCode = 401;
        response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\"";
    }
}