using System;
using System.Threading.Tasks;
using Tenon.Caching.Abstractions;

namespace Tenon.Hangfire.Extensions.Services;

/// <summary>
/// 登录尝试跟踪器
/// </summary>
public class LoginAttemptTracker : ILoginAttemptTracker
{
    private readonly ICacheProvider _cache;
    private const string KeyPrefix = "LoginAttempts_";
    private const string LockoutPrefix = "LoginLockout_";

    public LoginAttemptTracker(ICacheProvider cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public bool IsAccountLocked(string username)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentNullException(nameof(username));

        var lockoutKey = $"{LockoutPrefix}{username}";
        return _cache.Exists(lockoutKey);
    }

    public async Task<bool> IsAccountLockedAsync(string username)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentNullException(nameof(username));

        var lockoutKey = $"{LockoutPrefix}{username}";
        return await _cache.ExistsAsync(lockoutKey);
    }

    public int GetRemainingAttempts(string username, int maxAttempts)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentNullException(nameof(username));

        var key = $"{KeyPrefix}{username}";
        var result = _cache.Get<int>(key);
        var attempts = result.HasValue ? result.Value : 0;
        return Math.Max(0, maxAttempts - attempts);
    }

    public async Task<int> GetRemainingAttemptsAsync(string username, int maxAttempts)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentNullException(nameof(username));

        var key = $"{KeyPrefix}{username}";
        var result = await _cache.GetAsync<int>(key);
        var attempts = result.HasValue ? result.Value : 0;
        return Math.Max(0, maxAttempts - attempts);
    }

    public void RecordFailedAttempt(string username, int maxAttempts, TimeSpan lockoutDuration)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentNullException(nameof(username));

        var key = $"{KeyPrefix}{username}";
        var result = _cache.Get<int>(key);
        var attempts = result.HasValue ? result.Value + 1 : 1;

        _cache.Set(key, attempts, TimeSpan.FromHours(1));

        if (attempts >= maxAttempts)
        {
            var lockoutKey = $"{LockoutPrefix}{username}";
            _cache.Set(lockoutKey, true, lockoutDuration);
        }
    }

    public async Task RecordFailedAttemptAsync(string username, int maxAttempts, TimeSpan lockoutDuration)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentNullException(nameof(username));

        var key = $"{KeyPrefix}{username}";
        var result = await _cache.GetAsync<int>(key);
        var attempts = result.HasValue ? result.Value + 1 : 1;

        await _cache.SetAsync(key, attempts, TimeSpan.FromHours(1));

        if (attempts >= maxAttempts)
        {
            var lockoutKey = $"{LockoutPrefix}{username}";
            await _cache.SetAsync(lockoutKey, true, lockoutDuration);
        }
    }

    public void ResetFailedAttempts(string username)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentNullException(nameof(username));

        var key = $"{KeyPrefix}{username}";
        var lockoutKey = $"{LockoutPrefix}{username}";

        _cache.Remove(key);
        _cache.Remove(lockoutKey);
    }

    public async Task ResetFailedAttemptsAsync(string username)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentNullException(nameof(username));

        var key = $"{KeyPrefix}{username}";
        var lockoutKey = $"{LockoutPrefix}{username}";

        await _cache.RemoveAsync(key);
        await _cache.RemoveAsync(lockoutKey);
    }
} 