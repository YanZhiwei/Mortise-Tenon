using Tenon.Caching.InMemory;
using Tenon.Hangfire.Extensions.Caching;

namespace HangfireSample.Caching;

/// <summary>
/// Hangfire 内存缓存提供程序
/// </summary>
public sealed class HangfireMemoryCacheProvider : MemoryCacheProvider, IHangfireCacheProvider
{
    /// <summary>
    /// 创建一个 Hangfire 专用的内存缓存实例
    /// </summary>
    public HangfireMemoryCacheProvider() : base(
        cacheName: "HangfireCache",
        cacheMemoryLimitMegabytes: 100,
        physicalMemoryLimitPercentage: 10,
        pollingInterval: TimeSpan.FromMinutes(5))
    {
    }
} 