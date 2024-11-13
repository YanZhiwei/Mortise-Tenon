using System.Collections.Specialized;
using System.Runtime.Caching;
using Tenon.Caching.Abstractions;

namespace Tenon.Caching.InMemory;

public sealed class MemoryCacheProvider : ICacheProvider, IDisposable
{
    private readonly MemoryCache _cache;
    private readonly bool _isDefaultCache;

    /// <summary>
    ///     创建一个内存缓存实例
    /// </summary>
    /// <param name="cacheName">缓存Key</param>
    /// <param name="cacheMemoryLimitMegabytes">设置最大内存限制</param>
    /// <param name="physicalMemoryLimitPercentage">使用物理内存的限制百分比</param>
    /// <param name="pollingInterval">设置自动清理的间隔时间</param>
    public MemoryCacheProvider(string? cacheName = null, long? cacheMemoryLimitMegabytes = null,
        int? physicalMemoryLimitPercentage = null, TimeSpan? pollingInterval = null)
    {
        if (cacheName == null && cacheMemoryLimitMegabytes == null && physicalMemoryLimitPercentage == null &&
            pollingInterval == null)
        {
            _cache = MemoryCache.Default;
            _isDefaultCache = true;
        }
        else
        {
            var config = new NameValueCollection();
            if (cacheMemoryLimitMegabytes.HasValue)
                config.Add("CacheMemoryLimitMegabytes", cacheMemoryLimitMegabytes.Value.ToString());
            if (physicalMemoryLimitPercentage.HasValue)
                config.Add("PhysicalMemoryLimitPercentage", physicalMemoryLimitPercentage.Value.ToString());
            //周期性地检查缓存并清除已过期的内容，从而释放内存并保持缓存的有效性,默认间隔为 2 分钟
            config.Add("PollingInterval", (pollingInterval ?? TimeSpan.FromMinutes(2)).ToString());

            _cache = new MemoryCache(cacheName ?? nameof(MemoryCacheProvider), config);
            _isDefaultCache = false;
        }
    }

    public bool Set<T>(string cacheKey, T cacheValue, TimeSpan expiration)
    {
        if (string.IsNullOrEmpty(cacheKey) || cacheValue == null) return false;

        var cacheItemPolicy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.UtcNow.Add(expiration)
        };
        _cache.Set(cacheKey, cacheValue, cacheItemPolicy);
        return true;
    }

    public Task<bool> SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
    {
        return Task.FromResult(Set(cacheKey, cacheValue, expiration));
    }

    public CacheValue<T> Get<T>(string cacheKey)
    {
        var value = _cache.Get(cacheKey);
        return value is T typedValue
            ? new CacheValue<T>(typedValue, true)
            : CacheValue<T>.Null;
    }

    public Task<CacheValue<T>> GetAsync<T>(string cacheKey)
    {
        return Task.FromResult(Get<T>(cacheKey));
    }

    public bool Remove(string cacheKey)
    {
        return _cache.Remove(cacheKey) != null;
    }

    public Task<bool> RemoveAsync(string cacheKey)
    {
        return Task.FromResult(Remove(cacheKey));
    }

    public bool Exists(string cacheKey)
    {
        return _cache.Contains(cacheKey);
    }

    public Task<bool> ExistsAsync(string cacheKey)
    {
        return Task.FromResult(Exists(cacheKey));
    }

    public long RemoveAll(IEnumerable<string> cacheKeys)
    {
        long removedCount = 0;
        foreach (var key in cacheKeys)
            if (Remove(key))
                removedCount++;
        return removedCount;
    }

    public Task<long> RemoveAllAsync(IEnumerable<string> cacheKeys)
    {
        return Task.FromResult(RemoveAll(cacheKeys));
    }

    public Task KeysExpireAsync(IEnumerable<string> cacheKeys)
    {
        return Task.WhenAll(cacheKeys.Select(key => Task.Run(() => _cache.Remove(key))));
    }

    public Task KeysExpireAsync(IEnumerable<string> cacheKeys, TimeSpan expiration)
    {
        foreach (var key in cacheKeys)
            if (_cache.Get(key) is { } value)
                Set(key, value, expiration);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (!_isDefaultCache) _cache.Dispose();
    }
}