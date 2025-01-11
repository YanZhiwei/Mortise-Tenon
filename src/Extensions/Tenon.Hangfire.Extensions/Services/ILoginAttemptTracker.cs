using Tenon.Hangfire.Extensions.Configuration;

namespace Tenon.Hangfire.Extensions.Services;

/// <summary>
/// 登录尝试跟踪器接口
/// </summary>
public interface ILoginAttemptTracker
{
    /// <summary>
    /// 记录登录失败
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="options">认证选项</param>
    void RecordFailedAttempt(string username, AuthenticationOptions options);

    /// <summary>
    /// 重置登录失败次数
    /// </summary>
    /// <param name="username">用户名</param>
    void ResetAttempts(string username);

    /// <summary>
    /// 检查是否被锁定
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="options">认证选项</param>
    /// <returns>是否被锁定</returns>
    bool IsLockedOut(string username, AuthenticationOptions options);
} 