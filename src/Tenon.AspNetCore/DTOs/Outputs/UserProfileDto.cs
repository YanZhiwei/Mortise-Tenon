using System.ComponentModel;

namespace Tenon.AspNetCore.DTOs.Outputs;

/// <summary>
///     用户资料信息DTO
/// </summary>
public class UserProfileDto
{
    /// <summary>
    ///     用户ID
    /// </summary>
    [Description("用户ID")]
    public string Sub { get; set; } = string.Empty;


    /// <summary>
    ///     身份提供者
    /// </summary>
    [Description("身份提供者")]
    public string Idp { get; set; } = "local";


    /// <summary>
    ///     用户名
    /// </summary>
    [Description("用户名")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     是否为控制台管理员
    /// </summary>
    [Description("是否为控制台管理员")]
    public string ConsoleAdmin { get; set; } = "false";

    /// <summary>
    ///     首选用户名
    /// </summary>
    [Description("首选用户名")]
    public string PreferredUsername { get; set; } = string.Empty;

    /// <summary>
    ///     电话号码
    /// </summary>
    [Description("电话号码")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    ///     电话号码是否已验证
    /// </summary>
    [Description("电话号码是否已验证")]
    public bool PhoneNumberVerified { get; set; }
}