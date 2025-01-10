using System;
using System.Threading.Tasks;

namespace Tenon.Hangfire.Extensions.Services;

/// <summary>
/// 登录尝试跟踪器接口
/// </summary>
public interface ILoginAttemptTracker
{
    /// <summary>
    /// 检查账户是否被锁定
    /// </summary>
    /// <param name="username">用户名</param>
    /// <returns>如果账户被锁定返回 true，否则返回 false</returns>
    bool IsAccountLocked(string username);

    /// <summary>
    /// 检查账户是否被锁定（异步）
    /// </summary>
    /// <param name="username">用户名</param>
    /// <returns>如果账户被锁定返回 true，否则返回 false</returns>
    Task<bool> IsAccountLockedAsync(string username);

    /// <summary>
    /// 获取剩余的登录尝试次数
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="maxAttempts">最大允许尝试次数</param>
    /// <returns>剩余的尝试次数</returns>
    int GetRemainingAttempts(string username, int maxAttempts);

    /// <summary>
    /// 获取剩余的登录尝试次数（异步）
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="maxAttempts">最大允许尝试次数</param>
    /// <returns>剩余的尝试次数</returns>
    Task<int> GetRemainingAttemptsAsync(string username, int maxAttempts);

    /// <summary>
    /// 记录失败的登录尝试
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="maxAttempts">最大允许尝试次数</param>
    /// <param name="lockoutDuration">锁定持续时间</param>
    void RecordFailedAttempt(string username, int maxAttempts, TimeSpan lockoutDuration);

    /// <summary>
    /// 记录失败的登录尝试（异步）
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="maxAttempts">最大允许尝试次数</param>
    /// <param name="lockoutDuration">锁定持续时间</param>
    Task RecordFailedAttemptAsync(string username, int maxAttempts, TimeSpan lockoutDuration);

    /// <summary>
    /// 重置失败的登录尝试
    /// </summary>
    /// <param name="username">用户名</param>
    void ResetFailedAttempts(string username);

    /// <summary>
    /// 重置失败的登录尝试（异步）
    /// </summary>
    /// <param name="username">用户名</param>
    Task ResetFailedAttemptsAsync(string username);
} 