using Tenon.Hangfire.Extensions.Configuration;

namespace Tenon.Hangfire.Extensions.Services;

/// <summary>
///     密码验证器
/// </summary>
public class PasswordValidator : IPasswordValidator
{
    /// <summary>
    ///     验证密码
    /// </summary>
    /// <param name="password">密码</param>
    /// <param name="options">认证选项</param>
    /// <returns>验证结果</returns>
    public bool ValidatePassword(string password, AuthenticationOptions options)
    {
        // 首先验证密码是否匹配
        if (password != options.Password)
            return false;

        // 如果不启用密码复杂度验证，直接返回 true
        if (!options.EnablePasswordComplexity)
            return true;

        // 验证密码复杂度
        if (string.IsNullOrEmpty(password) || password.Length < options.MinPasswordLength)
            return false;

        if (options.RequireDigit && !password.Any(char.IsDigit))
            return false;

        if (options.RequireLowercase && !password.Any(char.IsLower))
            return false;

        if (options.RequireUppercase && !password.Any(char.IsUpper))
            return false;

        if (options.RequireSpecialCharacter && password.All(char.IsLetterOrDigit))
            return false;

        return true;
    }
}