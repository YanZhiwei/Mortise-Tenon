using Tenon.Caching.Abstractions;
using Tenon.Caching.InMemory;
using Tenon.Hangfire.Extensions.Caching;

namespace HangfireSample.Caching;

/// <summary>
///     Hangfire 内存缓存提供程序
/// </summary>
public sealed class HangfireMemoryCacheProvider : IHangfireCacheProvider, IDisposable
{
    private readonly MemoryCacheProvider _cache;

    /// <summary>
    ///     构造函数
    /// </summary>
    public HangfireMemoryCacheProvider()
    {
        _cache = new MemoryCacheProvider(
            cacheName: "HangfireCache",
            cacheMemoryLimitMegabytes: 100,
            physicalMemoryLimitPercentage: 10,
            pollingInterval: TimeSpan.FromMinutes(5));
    }

    /// <inheritdoc />
    public bool Set<T>(string cacheKey, T cacheValue, TimeSpan expiration)
    {
        return _cache.Set(cacheKey, cacheValue, expiration);
    }

    /// <inheritdoc />
    public Task<bool> SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
    {
        return _cache.SetAsync(cacheKey, cacheValue, expiration);
    }

    /// <inheritdoc />
    public CacheValue<T> Get<T>(string cacheKey)
    {
        return _cache.Get<T>(cacheKey);
    }

    /// <inheritdoc />
    public Task<CacheValue<T>> GetAsync<T>(string cacheKey)
    {
        return _cache.GetAsync<T>(cacheKey);
    }

    /// <inheritdoc />
    public bool Remove(string cacheKey)
    {
        return _cache.Remove(cacheKey);
    }

    /// <inheritdoc />
    public Task<bool> RemoveAsync(string cacheKey)
    {
        return _cache.RemoveAsync(cacheKey);
    }

    /// <inheritdoc />
    public bool Exists(string cacheKey)
    {
        return _cache.Exists(cacheKey);
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string cacheKey)
    {
        return _cache.ExistsAsync(cacheKey);
    }

    /// <inheritdoc />
    public long RemoveAll(IEnumerable<string> cacheKeys)
    {
        return _cache.RemoveAll(cacheKeys);
    }

    /// <inheritdoc />
    public Task<long> RemoveAllAsync(IEnumerable<string> cacheKeys)
    {
        return _cache.RemoveAllAsync(cacheKeys);
    }

    /// <inheritdoc />
    public Task KeysExpireAsync(IEnumerable<string> cacheKeys)
    {
        return _cache.KeysExpireAsync(cacheKeys);
    }

    /// <inheritdoc />
    public Task KeysExpireAsync(IEnumerable<string> cacheKeys, TimeSpan expiration)
    {
        return _cache.KeysExpireAsync(cacheKeys, expiration);
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public void Dispose()
    {
        if (_cache is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
} 