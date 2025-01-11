using Microsoft.Extensions.Logging;

namespace Tenon.Hangfire.Extensions.Services;

/// <summary>
///     Hangfire 服务
/// </summary>
internal class HangfireService
{
    private readonly ILogger<HangfireService> _logger;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public HangfireService(ILogger<HangfireService> logger)
    {
        _logger = logger;
    }
} 