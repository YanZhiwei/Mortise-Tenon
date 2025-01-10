using System.Net;
using Hangfire.Dashboard;
using Microsoft.Extensions.Logging;
using Tenon.Hangfire.Extensions.Configuration;

namespace Tenon.Hangfire.Extensions.Filters;

/// <summary>
///     Hangfire IP 白名单认证过滤器
/// </summary>
public sealed class HangfireIpAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly IpAuthorizationOptions _options;
    private readonly ILogger<HangfireIpAuthorizationFilter> _logger;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="options">IP 认证配置选项</param>
    /// <param name="logger">日志记录器</param>
    public HangfireIpAuthorizationFilter(IpAuthorizationOptions options, ILogger<HangfireIpAuthorizationFilter> logger)
    {
        _options = options;
        _logger = logger;
    }

    /// <summary>
    ///     授权验证
    /// </summary>
    /// <param name="context">仪表板上下文</param>
    /// <returns>是否授权通过</returns>
    public bool Authorize(DashboardContext context)
    {
        if (!_options.Enabled)
        {
            return true;
        }

        var httpContext = context.GetHttpContext();
        var remoteIp = httpContext.Connection.RemoteIpAddress;

        if (remoteIp == null)
        {
            _logger.LogWarning("无法获取远程 IP 地址");
            return false;
        }

        // 检查单个 IP 地址
        if (_options.AllowedIPs.Contains(remoteIp.ToString()))
        {
            return true;
        }

        // 检查 IP 范围
        foreach (var range in _options.AllowedIpRanges)
        {
            if (IsIpInRange(remoteIp, range))
            {
                return true;
            }
        }

        _logger.LogWarning("IP 地址不在白名单中: {RemoteIp}", remoteIp);
        return false;
    }

    /// <summary>
    ///     检查 IP 是否在指定范围内
    /// </summary>
    /// <param name="ip">IP 地址</param>
    /// <param name="cidr">CIDR 格式的 IP 范围</param>
    /// <returns>是否在范围内</returns>
    private bool IsIpInRange(IPAddress ip, string cidr)
    {
        try
        {
            var parts = cidr.Split('/');
            if (parts.Length != 2)
            {
                _logger.LogWarning("无效的 CIDR 格式: {Cidr}", cidr);
                return false;
            }

            var networkAddress = IPAddress.Parse(parts[0]);
            var prefixLength = int.Parse(parts[1]);

            var ipBytes = ip.GetAddressBytes();
            var networkBytes = networkAddress.GetAddressBytes();

            if (ipBytes.Length != networkBytes.Length)
            {
                return false; // IP 版本不匹配（IPv4 vs IPv6）
            }

            var networkMask = CreateNetworkMask(prefixLength, networkBytes.Length);
            for (var i = 0; i < ipBytes.Length; i++)
            {
                if ((ipBytes[i] & networkMask[i]) != (networkBytes[i] & networkMask[i]))
                {
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查 IP 范围时发生错误: {Cidr}", cidr);
            return false;
        }
    }

    /// <summary>
    ///     创建网络掩码
    /// </summary>
    /// <param name="prefixLength">前缀长度</param>
    /// <param name="addressLength">地址长度</param>
    /// <returns>网络掩码</returns>
    private static byte[] CreateNetworkMask(int prefixLength, int addressLength)
    {
        var mask = new byte[addressLength];
        for (var i = 0; i < addressLength; i++)
        {
            if (prefixLength >= 8)
            {
                mask[i] = 0xFF;
                prefixLength -= 8;
            }
            else if (prefixLength > 0)
            {
                mask[i] = (byte)(0xFF << (8 - prefixLength));
                prefixLength = 0;
            }
            else
            {
                mask[i] = 0x00;
            }
        }
        return mask;
    }
}