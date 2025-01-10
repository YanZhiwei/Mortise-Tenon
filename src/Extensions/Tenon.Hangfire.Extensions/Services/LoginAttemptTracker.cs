using Microsoft.Extensions.Caching.Memory;
using Tenon.Hangfire.Extensions.Configuration;

namespace Tenon.Hangfire.Extensions.Services;

/// <summary>
///     登录尝试跟踪器实现
/// </summary>
public class LoginAttemptTracker : ILoginAttemptTracker
{
    private readonly IMemoryCache _cache;
    private readonly AuthenticationOptions _options;
    private const string AttemptPrefix = "LoginAttempt_";
    private const string LockoutPrefix = "Lockout_";

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="cache">内存缓存</param>
    /// <param name="options">认证配置选项</param>
    public LoginAttemptTracker(IMemoryCache cache, AuthenticationOptions options)
    {
        _cache = cache;
        _options = options;
    }

    /// <summary>
    ///     记录失败的登录尝试
    /// </summary>
    /// <param name="username">用户名</param>
    public void RecordFailedAttempt(string username)
    {
        var attempts = GetCurrentAttempts(username);
        attempts++;

        if (attempts >= _options.MaxLoginAttempts)
        {
            // 设置账户锁定
            _cache.Set($"{LockoutPrefix}{username}", true,
                TimeSpan.FromMinutes(_options.LockoutDuration));
        }

        // 更新失败次数
        _cache.Set($"{AttemptPrefix}{username}", attempts,
            TimeSpan.FromMinutes(_options.LockoutDuration));
    }

    /// <summary>
    ///     重置失败的登录尝试
    /// </summary>
    /// <param name="username">用户名</param>
    public void ResetFailedAttempts(string username)
    {
        _cache.Remove($"{AttemptPrefix}{username}");
        _cache.Remove($"{LockoutPrefix}{username}");
    }

    /// <summary>
    ///     检查账户是否被锁定
    /// </summary>
    /// <param name="username">用户名</param>
    /// <returns>是否被锁定</returns>
    public bool IsAccountLocked(string username)
    {
        return _cache.TryGetValue($"{LockoutPrefix}{username}", out bool _);
    }

    /// <summary>
    ///     获取剩余的登录尝试次数
    /// </summary>
    /// <param name="username">用户名</param>
    /// <returns>剩余尝试次数</returns>
    public int GetRemainingAttempts(string username)
    {
        var attempts = GetCurrentAttempts(username);
        return Math.Max(0, _options.MaxLoginAttempts - attempts);
    }

    private int GetCurrentAttempts(string username)
    {
        return _cache.TryGetValue($"{AttemptPrefix}{username}", out int attempts) ? attempts : 0;
    }
} 