namespace Tenon.Hangfire.Extensions.Configuration;

/// <summary>
///     认证配置选项
/// </summary>
public class AuthenticationOptions
{
    /// <summary>
    ///     认证类型
    /// </summary>
    public AuthenticationType AuthType { get; set; } = AuthenticationType.Basic;

    /// <summary>
    ///     是否启用密码复杂度验证
    /// </summary>
    public bool EnablePasswordComplexity { get; set; } = true;

    /// <summary>
    ///     最小密码长度
    /// </summary>
    public int MinPasswordLength { get; set; } = 8;

    /// <summary>
    ///     是否要求包含数字
    /// </summary>
    public bool RequireDigit { get; set; } = true;

    /// <summary>
    ///     是否要求包含小写字母
    /// </summary>
    public bool RequireLowercase { get; set; } = true;

    /// <summary>
    ///     是否要求包含大写字母
    /// </summary>
    public bool RequireUppercase { get; set; } = true;

    /// <summary>
    ///     是否要求包含特殊字符
    /// </summary>
    public bool RequireSpecialCharacter { get; set; } = true;

    /// <summary>
    ///     登录失败最大尝试次数
    /// </summary>
    public int MaxLoginAttempts { get; set; } = 5;

    /// <summary>
    ///     登录失败锁定时间（分钟）
    /// </summary>
    public int LockoutDuration { get; set; } = 30;

    /// <summary>
    ///     JWT 配置（当 AuthType 为 JWT 时使用）
    /// </summary>
    public JwtOptions? JwtOptions { get; set; }
} 