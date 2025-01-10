using System;
using System.Linq;
using Tenon.Hangfire.Extensions.Configuration;

namespace Tenon.Hangfire.Extensions.Services;

/// <summary>
/// 密码验证器实现
/// </summary>
public class PasswordValidator : IPasswordValidator
{
    private readonly AuthenticationOptions _options;

    public PasswordValidator(AuthenticationOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// 验证密码是否符合复杂度要求
    /// </summary>
    /// <param name="password">待验证的密码</param>
    /// <returns>验证结果</returns>
    public PasswordValidationResult ValidatePassword(string password)
    {
        var result = new PasswordValidationResult();

        if (string.IsNullOrEmpty(password))
        {
            result.AddError("密码不能为空");
            return result;
        }

        // 验证密码长度
        if (password.Length < _options.MinPasswordLength)
        {
            result.AddError($"密码长度不能小于 {_options.MinPasswordLength} 个字符");
        }

        // 验证是否包含数字
        if (_options.RequireDigit && !password.Any(char.IsDigit))
        {
            result.AddError("密码必须包含至少一个数字");
        }

        // 验证是否包含小写字母
        if (_options.RequireLowercase && !password.Any(char.IsLower))
        {
            result.AddError("密码必须包含至少一个小写字母");
        }

        // 验证是否包含大写字母
        if (_options.RequireUppercase && !password.Any(char.IsUpper))
        {
            result.AddError("密码必须包含至少一个大写字母");
        }

        // 验证是否包含特殊字符
        if (_options.RequireSpecialCharacter && !password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            result.AddError("密码必须包含至少一个特殊字符");
        }

        return result;
    }
} 