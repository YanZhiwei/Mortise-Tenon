using System.Text.RegularExpressions;
using Tenon.Hangfire.Extensions.Configuration;

namespace Tenon.Hangfire.Extensions.Services;

/// <summary>
///     密码验证器实现
/// </summary>
public class PasswordValidator : IPasswordValidator
{
    /// <summary>
    ///     验证密码是否符合复杂度要求
    /// </summary>
    /// <param name="password">密码</param>
    /// <param name="options">认证配置选项</param>
    /// <returns>验证结果</returns>
    public PasswordValidationResult ValidatePassword(string password, AuthenticationOptions options)
    {
        var result = new PasswordValidationResult { IsValid = true };

        if (!options.EnablePasswordComplexity)
        {
            return result;
        }

        if (string.IsNullOrEmpty(password))
        {
            result.IsValid = false;
            result.Errors.Add("密码不能为空");
            return result;
        }

        if (password.Length < options.MinPasswordLength)
        {
            result.IsValid = false;
            result.Errors.Add($"密码长度不能小于 {options.MinPasswordLength} 个字符");
        }

        if (options.RequireDigit && !password.Any(char.IsDigit))
        {
            result.IsValid = false;
            result.Errors.Add("密码必须包含数字");
        }

        if (options.RequireLowercase && !password.Any(char.IsLower))
        {
            result.IsValid = false;
            result.Errors.Add("密码必须包含小写字母");
        }

        if (options.RequireUppercase && !password.Any(char.IsUpper))
        {
            result.IsValid = false;
            result.Errors.Add("密码必须包含大写字母");
        }

        if (options.RequireSpecialCharacter && !Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]"))
        {
            result.IsValid = false;
            result.Errors.Add("密码必须包含特殊字符");
        }

        return result;
    }
} 