using Tenon.Hangfire.Extensions.Configuration;

namespace Tenon.Hangfire.Extensions.Services;

/// <summary>
///     密码验证器接口
/// </summary>
public interface IPasswordValidator
{
    /// <summary>
    ///     验证密码是否符合复杂度要求
    /// </summary>
    /// <param name="password">密码</param>
    /// <param name="options">认证配置选项</param>
    /// <returns>验证结果</returns>
    PasswordValidationResult ValidatePassword(string password, AuthenticationOptions options);
}

/// <summary>
///     密码验证结果
/// </summary>
public class PasswordValidationResult
{
    /// <summary>
    ///     是否验证通过
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    ///     错误消息
    /// </summary>
    public List<string> Errors { get; set; } = new();
} 