namespace Tenon.Hangfire.Extensions.Services;

/// <summary>
///     登录尝试跟踪器接口
/// </summary>
public interface ILoginAttemptTracker
{
    /// <summary>
    ///     记录失败的登录尝试
    /// </summary>
    /// <param name="username">用户名</param>
    void RecordFailedAttempt(string username);

    /// <summary>
    ///     重置失败的登录尝试
    /// </summary>
    /// <param name="username">用户名</param>
    void ResetFailedAttempts(string username);

    /// <summary>
    ///     检查账户是否被锁定
    /// </summary>
    /// <param name="username">用户名</param>
    /// <returns>是否被锁定</returns>
    bool IsAccountLocked(string username);

    /// <summary>
    ///     获取剩余的登录尝试次数
    /// </summary>
    /// <param name="username">用户名</param>
    /// <returns>剩余尝试次数</returns>
    int GetRemainingAttempts(string username);
} 