using System;
using System.Linq;
using Tenon.Caching.Abstractions;
using Tenon.Hangfire.Extensions.Caching;
using Tenon.Hangfire.Extensions.Configuration;

namespace Tenon.Hangfire.Extensions.Services;

/// <summary>
///     密码验证器
/// </summary>
public class PasswordValidator : IPasswordValidator
{
    private readonly IHangfireCacheProvider _cacheProvider;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="cacheProvider">缓存提供程序</param>
    public PasswordValidator(IHangfireCacheProvider cacheProvider)
    {
        _cacheProvider = cacheProvider;
    }

    /// <summary>
    ///     验证密码
    /// </summary>
    /// <param name="password">密码</param>
    /// <param name="options">认证选项</param>
    /// <returns>验证结果</returns>
    public bool ValidatePassword(string password, AuthenticationOptions options)
    {
        if (!options.EnablePasswordComplexity)
            return true;

        if (string.IsNullOrEmpty(password) || password.Length < options.MinPasswordLength)
            return false;

        if (options.RequireDigit && !password.Any(char.IsDigit))
            return false;

        if (options.RequireLowercase && !password.Any(char.IsLower))
            return false;

        if (options.RequireUppercase && !password.Any(char.IsUpper))
            return false;

        if (options.RequireSpecialCharacter && !password.Any(c => !char.IsLetterOrDigit(c)))
            return false;

        return true;
    }
} 