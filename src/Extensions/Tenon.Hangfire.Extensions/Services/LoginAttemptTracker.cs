using System;
using System.Threading.Tasks;
using Tenon.Caching.Abstractions;
using Tenon.Hangfire.Extensions.Caching;
using Tenon.Hangfire.Extensions.Configuration;

namespace Tenon.Hangfire.Extensions.Services;

/// <summary>
///     登录尝试跟踪器
/// </summary>
public class LoginAttemptTracker : ILoginAttemptTracker
{
    private readonly IHangfireCacheProvider _cacheProvider;
    private const string KeyPrefix = "hangfire:login:attempt:";

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="cacheProvider">缓存提供程序</param>
    public LoginAttemptTracker(IHangfireCacheProvider cacheProvider)
    {
        _cacheProvider = cacheProvider;
    }

    /// <summary>
    ///     记录登录失败
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="options">认证选项</param>
    public void RecordFailedAttempt(string username, AuthenticationOptions options)
    {
        var key = GetKey(username);
        var attempts = GetAttempts(key);
        attempts++;

        _cacheProvider.Set(key, attempts, TimeSpan.FromMinutes(options.LockoutDuration));
    }

    /// <summary>
    ///     重置登录失败次数
    /// </summary>
    /// <param name="username">用户名</param>
    public void ResetAttempts(string username)
    {
        var key = GetKey(username);
        _cacheProvider.Remove(key);
    }

    /// <summary>
    ///     检查是否被锁定
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="options">认证选项</param>
    /// <returns>是否被锁定</returns>
    public bool IsLockedOut(string username, AuthenticationOptions options)
    {
        var key = GetKey(username);
        var attempts = GetAttempts(key);
        return attempts >= options.MaxLoginAttempts;
    }

    private int GetAttempts(string key)
    {
        var value = _cacheProvider.Get<int>(key);
        return value.HasValue ? value.Value : 0;
    }

    private static string GetKey(string username) => $"{KeyPrefix}{username}";
} 