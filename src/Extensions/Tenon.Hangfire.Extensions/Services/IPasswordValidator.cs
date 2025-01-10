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
    /// <param name="password">待验证的密码</param>
    /// <returns>验证结果</returns>
    PasswordValidationResult ValidatePassword(string password);
} 