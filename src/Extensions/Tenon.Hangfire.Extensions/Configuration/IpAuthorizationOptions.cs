namespace Tenon.Hangfire.Extensions.Configuration;

/// <summary>
///     IP 白名单配置选项
/// </summary>
public class IpAuthorizationOptions
{
    private List<string> _allowedIpRanges = new();

    private List<string> _allowedIPs = new();

    /// <summary>
    ///     是否启用 IP 白名单
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     允许的 IP 地址列表
    /// </summary>
    public List<string> AllowedIPs
    {
        get => _allowedIPs;
        set
        {
            _allowedIPs = value ?? [];
            if (!_allowedIPs.Contains("127.0.0.1"))
                _allowedIPs.Add("127.0.0.1");
            if (!_allowedIPs.Contains("::1"))
                _allowedIPs.Add("::1");
        }
    }

    /// <summary>
    ///     允许的 IP 地址范围列表（CIDR 格式，如：192.168.1.0/24）
    /// </summary>
    public List<string> AllowedIpRanges
    {
        get => _allowedIpRanges;
        set => _allowedIpRanges = value ?? [];
    }

    /// <summary>
    ///     验证配置是否有效
    /// </summary>
    /// <returns>是否有效</returns>
    public bool IsValid()
    {
        return Enabled && (AllowedIPs.Count > 0 || AllowedIpRanges.Count > 0);
    }
}