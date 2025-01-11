using System.Net;
using Hangfire.Dashboard;
using Microsoft.Extensions.Logging;
using Tenon.Hangfire.Extensions.Configuration;

namespace Tenon.Hangfire.Extensions.Filters;

/// <summary>
///     Hangfire IP 授权过滤器
/// </summary>
public class HangfireIpAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly ILogger<HangfireIpAuthorizationFilter> _logger;
    private readonly IpAuthorizationOptions _options;
    private readonly bool _skipBasicAuthIfAuthorized;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="options">IP 授权配置选项</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="skipBasicAuthIfAuthorized">IP 验证通过时是否跳过基本认证</param>
    public HangfireIpAuthorizationFilter(
        IpAuthorizationOptions options,
        ILogger<HangfireIpAuthorizationFilter> logger,
        bool skipBasicAuthIfAuthorized = false)
    {
        _options = options;
        _logger = logger;
        _skipBasicAuthIfAuthorized = skipBasicAuthIfAuthorized;
    }

    /// <summary>
    ///     授权
    /// </summary>
    /// <param name="context">授权上下文</param>
    /// <returns>是否授权通过</returns>
    public bool Authorize(DashboardContext context)
    {
        _logger.LogInformation("开始 IP 授权验证");

        if (!_options.Enabled)
        {
            _logger.LogInformation("IP 授权未启用");
            return true;
        }

        var httpContext = context.GetHttpContext();
        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();

        _logger.LogInformation("当前访问 IP: {RemoteIp}", remoteIp);

        if (string.IsNullOrEmpty(remoteIp))
        {
            _logger.LogWarning("无法获取访问 IP");
            return false;
        }

        _logger.LogInformation("允许的 IP 列表: {AllowedIPs}", string.Join(", ", _options.AllowedIPs));
        _logger.LogInformation("允许的 IP 范围: {AllowedIpRanges}", string.Join(", ", _options.AllowedIpRanges));

        var isAuthorized = false;

        // 检查是否在允许的 IP 列表中
        if (_options.AllowedIPs.Contains(remoteIp))
        {
            _logger.LogInformation("IP {RemoteIp} 在允许列表中", remoteIp);
            isAuthorized = true;
        }
        // 检查是否在允许的 IP 范围内
        else if (_options.AllowedIpRanges.Any(range => IsIpInRange(remoteIp, range)))
        {
            _logger.LogInformation("IP {RemoteIp} 在允许范围内", remoteIp);
            isAuthorized = true;
        }
        else
        {
            _logger.LogWarning("IP {RemoteIp} 未被授权访问", remoteIp);
        }

        // 如果 IP 验证通过且配置为跳过基本认证，则设置标记
        if (isAuthorized && _skipBasicAuthIfAuthorized)
        {
            httpContext.Items["SkipBasicAuth"] = true;
            _logger.LogInformation("IP 验证通过，将跳过基本认证");
        }

        return isAuthorized;
    }

    private bool IsIpInRange(string ip, string range)
    {
        try
        {
            var networkParts = range.Split('/');
            if (networkParts.Length != 2)
                return false;

            var networkAddress = IPAddress.Parse(networkParts[0]);
            var prefixLength = int.Parse(networkParts[1]);
            var ipAddress = IPAddress.Parse(ip);

            var networkBytes = networkAddress.GetAddressBytes();
            var ipBytes = ipAddress.GetAddressBytes();

            if (networkBytes.Length != ipBytes.Length)
                return false;

            var mask = CreateMask(prefixLength, networkBytes.Length);
            return IsMatch(ipBytes, networkBytes, mask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查 IP 范围时发生错误");
            return false;
        }
    }

    private static byte[] CreateMask(int prefixLength, int length)
    {
        var mask = new byte[length];
        for (var i = 0; i < length; i++)
            if (prefixLength >= 8)
            {
                mask[i] = 0xFF;
                prefixLength -= 8;
            }
            else if (prefixLength > 0)
            {
                mask[i] = (byte) (0xFF << (8 - prefixLength));
                prefixLength = 0;
            }
            else
            {
                mask[i] = 0x00;
            }

        return mask;
    }

    private static bool IsMatch(byte[] ip, byte[] network, byte[] mask)
    {
        for (var i = 0; i < ip.Length; i++)
            if ((ip[i] & mask[i]) != (network[i] & mask[i]))
                return false;
        return true;
    }
}