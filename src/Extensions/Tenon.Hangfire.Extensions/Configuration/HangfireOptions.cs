namespace Tenon.Hangfire.Extensions.Configuration;

/// <summary>
///     Hangfire 配置选项
/// </summary>
public class HangfireOptions
{
    /// <summary>
    ///     仪表板路径
    /// </summary>
    public string Path { get; set; } = "/hangfire";

    /// <summary>
    ///     仪表板标题
    /// </summary>
    public string DashboardTitle { get; set; } = "任务调度中心";

    /// <summary>
    ///     是否忽略防伪令牌
    /// </summary>
    public bool IgnoreAntiforgeryToken { get; set; }

    /// <summary>
    ///     IP 授权配置
    /// </summary>
    public IpAuthorizationOptions IpAuthorization { get; set; } = new();

    /// <summary>
    ///     认证配置
    /// </summary>
    public AuthenticationOptions Authentication { get; set; } = new();

    /// <summary>
    ///     IP 验证通过时是否跳过基本认证
    /// </summary>
    public bool SkipBasicAuthenticationIfIpAuthorized { get; set; }
}