namespace Tenon.Hangfire.Extensions.Configuration;

/// <summary>
///     Hangfire 配置选项
/// </summary>
public class HangfireOptions
{
    /// <summary>
    ///     仪表板路径
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    ///     仪表板标题
    /// </summary>
    public string DashboardTitle { get; set; } = "Hangfire";

    /// <summary>
    ///     认证配置
    /// </summary>
    public AuthenticationOptions Authentication { get; set; } = new();

    /// <summary>
    ///     IP 白名单配置
    /// </summary>
    public IpAuthorizationOptions IpAuthorization { get; set; } = new();
}