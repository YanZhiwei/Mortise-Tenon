namespace FluentValidationSample.Models;

/// <summary>
/// 用户注册结果 DTO
/// </summary>
public class UserRegistrationResultDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 电子邮件
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 注册时间
    /// </summary>
    public DateTime RegisterTime { get; set; }
} 