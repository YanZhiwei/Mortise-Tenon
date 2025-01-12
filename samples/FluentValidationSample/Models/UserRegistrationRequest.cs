namespace FluentValidationSample.Models;

/// <summary>
/// 用户注册请求模型
/// </summary>
public class UserRegistrationRequest
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = default!;

    /// <summary>
    /// 电子邮件
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = default!;

    /// <summary>
    /// 确认密码
    /// </summary>
    public string ConfirmPassword { get; set; } = default!;

    /// <summary>
    /// 年龄
    /// </summary>
    public int Age { get; set; }
} 