namespace Tenon.Hangfire.Extensions.Configuration;

/// <summary>
///     IP 白名单配置选项
/// </summary>
public class IpAuthorizationOptions
{
    /// <summary>
    ///     是否启用 IP 白名单
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     允许的 IP 地址列表
    /// </summary>
    public List<string> AllowedIPs { get; set; } = new() { "127.0.0.1", "::1" };

    /// <summary>
    ///     允许的 IP 地址范围列表（CIDR 格式，如：192.168.1.0/24）
    /// </summary>
    public List<string> AllowedIpRanges { get; set; } = new();
} 