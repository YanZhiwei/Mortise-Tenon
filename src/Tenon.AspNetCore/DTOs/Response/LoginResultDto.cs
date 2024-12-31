using System.ComponentModel;

namespace Tenon.AspNetCore.DTOs.Response;

/// <summary>
/// 登录结果数据传输对象
/// </summary>
/// <remarks>
/// 包含登录成功后返回的用户信息、访问令牌等数据。
/// </remarks>
public class LoginResultDto
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    /// <value>用于身份验证的 JWT 令牌</value>
    [Description("访问令牌")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 令牌类型
    /// </summary>
    /// <value>令牌的类型，通常为 "Bearer"</value>
    [Description("令牌类型")]
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// 过期时间（秒）
    /// </summary>
    /// <value>令牌的有效期（以秒为单位）</value>
    [Description("过期时间（秒）")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// 刷新令牌
    /// </summary>
    /// <value>用于刷新访问令牌的令牌</value>
    [Description("刷新令牌")]
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// 用户标识
    /// </summary>
    /// <value>用户的唯一标识符</value>
    [Description("用户标识")]
    public string Sub { get; set; } = string.Empty;

    /// <summary>
    /// 身份提供者
    /// </summary>
    /// <value>提供身份验证的服务提供者</value>
    [Description("身份提供者")]
    public string Idp { get; set; } = "local";

    /// <summary>
    /// 用户名
    /// </summary>
    /// <value>用户的登录名</value>
    [Description("用户名")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 首选用户名
    /// </summary>
    /// <value>用户的显示名称</value>
    [Description("首选用户名")]
    public string PreferredUsername { get; set; } = string.Empty;

    /// <summary>
    /// 电话号码
    /// </summary>
    /// <value>用户的联系电话</value>
    [Description("电话号码")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// 电话号码是否已验证
    /// </summary>
    /// <value>指示电话号码是否已经过验证</value>
    [Description("电话号码是否已验证")]
    public bool PhoneNumberVerified { get; set; }

    /// <summary>
    /// 获取显示名称
    /// </summary>
    /// <returns>优先返回首选用户名，如果为空则返回用户名</returns>
    public string GetDisplayName() => !string.IsNullOrWhiteSpace(PreferredUsername) ? PreferredUsername : Name;
}