using System;
using System.Runtime.Caching;
using System.Collections.Specialized;
using Tenon.Hangfire.Extensions.Caching;

namespace HangfireSample.Caching;

/// <summary>
///     Hangfire 内存缓存提供程序
/// </summary>
public sealed class HangfireMemoryCacheProvider : IHangfireCacheProvider, IDisposable
{
    private readonly MemoryCache _cache;

    /// <summary>
    ///     构造函数
    /// </summary>
    public HangfireMemoryCacheProvider()
    {
        var config = new NameValueCollection
        {
            { "CacheMemoryLimitMegabytes", "100" },
            { "PhysicalMemoryLimitPercentage", "10" },
            { "PollingInterval", TimeSpan.FromMinutes(5).ToString() }
        };

        _cache = new MemoryCache("HangfireCache", config);
    }

    /// <inheritdoc />
    public void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration)
    {
        if (string.IsNullOrEmpty(cacheKey) || cacheValue == null) return;

        var cacheItemPolicy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.UtcNow.Add(expiration)
        };
        _cache.Set(cacheKey, cacheValue, cacheItemPolicy);
    }

    /// <inheritdoc />
    public T? Get<T>(string cacheKey)
    {
        var value = _cache.Get(cacheKey);
        return value is T typedValue ? typedValue : default;
    }

    /// <inheritdoc />
    public void Remove(string cacheKey)
    {
        _cache.Remove(cacheKey);
    }

    /// <inheritdoc />
    public bool Exists(string cacheKey)
    {
        return _cache.Contains(cacheKey);
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public void Dispose()
    {
        _cache.Dispose();
    }
} 