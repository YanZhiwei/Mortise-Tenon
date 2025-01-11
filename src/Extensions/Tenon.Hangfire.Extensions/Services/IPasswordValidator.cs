using Tenon.Hangfire.Extensions.Configuration;

namespace Tenon.Hangfire.Extensions.Services;

/// <summary>
/// 密码验证器接口
/// </summary>
public interface IPasswordValidator
{
    /// <summary>
    /// 验证密码
    /// </summary>
    /// <param name="password">密码</param>
    /// <param name="options">认证选项</param>
    /// <returns>验证结果</returns>
    bool ValidatePassword(string password, AuthenticationOptions options);
} 