namespace Tenon.Hangfire.Extensions.Configuration;

/// <summary>
///     Hangfire 配置选项
/// </summary>
public class HangfireOptions
{
    /// <summary>
    ///     用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    ///     密码
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    ///     仪表板路径
    /// </summary>
    public string Path { get; set; } = string.Empty;

    public string DashboardTitle { get; set; } = "Hangfire";
}