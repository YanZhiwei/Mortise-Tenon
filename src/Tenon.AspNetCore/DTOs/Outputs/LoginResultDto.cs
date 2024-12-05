using System.ComponentModel;

namespace Tenon.AspNetCore.DTOs.Outputs;

/// <summary>
///     登录结果DTO，包含令牌和用户资料
/// </summary>
public class LoginResultDto
{
    /// <summary>
    ///     访问令牌
    /// </summary>
    [Description("访问令牌")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    ///     刷新令牌
    /// </summary>
    [Description("刷新令牌")]
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    ///     令牌类型（例如，"Bearer"）
    /// </summary>
    [Description("令牌类型")]
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    ///     令牌范围
    /// </summary>
    [Description("令牌范围")]
    public List<string> Scope { get; set; } = ["openid", "profile", "apigateway", "offline_access"];

    /// <summary>
    ///     用户资料信息
    /// </summary>
    [Description("用户资料信息")]
    public UserProfileDto Profile { get; set; } = new();

    /// <summary>
    ///     令牌过期时间戳（Unix时间戳）
    /// </summary>
    [Description("令牌过期时间戳")]
    public long ExpiresAt { get; set; }

    /// <summary>
    ///     认证时间（ISO 8601格式）
    /// </summary>
    [Description("认证时间")]
    public string AuthTimeISO { get; set; } = string.Empty;

    /// <summary>
    ///     身份提供者
    /// </summary>
    [Description("身份提供者")]
    public string Idp { get; set; } = "local";

    /// <summary>
    ///     认证方法
    /// </summary>
    [Description("认证方法")]
    public List<string> Amr { get; set; } = ["pwd"];

    /// <summary>
    ///     默认时区ID
    /// </summary>
    [Description("默认时区ID")]
    public string DefaultTimezoneId { get; set; } = "China Standard Time";

    /// <summary>
    ///     默认时区偏移
    /// </summary>
    [Description("默认时区偏移")]
    public string DefaultTimezoneOffset { get; set; } = "+08:00";

    /// <summary>
    ///     令牌过期时间戳（ISO 8601格式）
    /// </summary>
    [Description("令牌过期时间戳")]
    public string ExpiresAtISO { get; set; } = string.Empty;
}