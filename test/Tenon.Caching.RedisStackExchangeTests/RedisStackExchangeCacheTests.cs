using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tenon.Caching.Abstractions;
using Tenon.Caching.RedisStackExchange.Extensions;
using Tenon.Helper.Internal;
using Tenon.Serialization.Json.Extensions;

namespace Tenon.Caching.RedisStackExchangeTests;

/// <summary>
///     Redis 缓存测试类
/// </summary>
[TestClass]
public class RedisStackExchangeCacheTests
{
    private ICacheProvider _cacheProvider;
    private ICacheProvider _keyedCacheProvider;
    private string _serviceKey;
    private IServiceProvider _serviceProvider;

    /// <summary>
    ///     初始化测试环境
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        _serviceKey = nameof(RedisStackExchangeCacheTests);
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false)
            .Build();

        var services = new ServiceCollection();
        services.AddSystemTextJsonSerializer();

        // 添加缓存服务
        services.AddRedisStackExchangeCache(configuration.GetSection("RedisCache:Redis"))
               .AddKeyedRedisStackExchangeCache(_serviceKey, configuration.GetSection("RedisCache:Redis"));

        _serviceProvider = services.BuildServiceProvider();
        _cacheProvider = _serviceProvider.GetRequiredService<ICacheProvider>();
        _keyedCacheProvider = _serviceProvider.GetRequiredKeyedService<ICacheProvider>(_serviceKey);
    }

    /// <summary>
    ///     测试设置和获取缓存值
    /// </summary>
    [TestMethod]
    public void Set_And_Get_Should_Work()
    {
        // Arrange
        var key = $"test_key_{RandomHelper.NextString(6, true)}";
        var value = "test_value";
        var expiration = TimeSpan.FromMinutes(5);

        // Act
        var setResult = _cacheProvider.Set(key, value, expiration);
        var getValue = _cacheProvider.Get<string>(key);

        // Assert
        Assert.IsTrue(setResult);
        Assert.IsTrue(getValue.HasValue);
        Assert.AreEqual(value, getValue.Value);
    }

    /// <summary>
    ///     测试检查缓存键是否存在
    /// </summary>
    [TestMethod]
    public void Exists_Should_Work()
    {
        // Arrange
        var key = "test_key_exists";
        var value = "test_value_exists";
        var expiration = TimeSpan.FromMinutes(5);
        _cacheProvider.Set(key, value, expiration);

        // Act
        var existsResult = _cacheProvider.Exists(key);

        // Assert
        Assert.IsTrue(existsResult);
    }

    /// <summary>
    ///     测试异步设置和获取缓存值
    /// </summary>
    [TestMethod]
    public async Task SetAsync_And_GetAsync_Should_Work()
    {
        // Arrange
        var key = $"test_key_async_{RandomHelper.NextString(6, true)}";
        var value = "test_value_async";
        var expiration = TimeSpan.FromMinutes(5);

        // Act
        var setResult = await _cacheProvider.SetAsync(key, value, expiration);
        var getValue = await _cacheProvider.GetAsync<string>(key);

        // Assert
        Assert.IsTrue(setResult);
        Assert.IsTrue(getValue.HasValue);
        Assert.AreEqual(value, getValue.Value);
    }

    /// <summary>
    ///     测试带键的缓存设置和获取
    /// </summary>
    [TestMethod]
    public void KeyedSet_And_Get_Should_Work()
    {
        // Arrange
        var key = $"test_keyed_key_{RandomHelper.NextString(6, true)}";
        var value = "test_keyed_value";
        var expiration = TimeSpan.FromMinutes(5);

        // Act
        var setResult = _keyedCacheProvider.Set(key, value, expiration);
        var getValue = _keyedCacheProvider.Get<string>(key);

        // Assert
        Assert.IsTrue(setResult);
        Assert.IsTrue(getValue.HasValue);
        Assert.AreEqual(value, getValue.Value);
    }

    /// <summary>
    ///     测试带键的异步缓存设置和获取
    /// </summary>
    [TestMethod]
    public async Task KeyedSetAsync_And_GetAsync_Should_Work()
    {
        // Arrange
        var key = $"test_keyed_key_async_{RandomHelper.NextString(6, true)}";
        var value = "test_keyed_value_async";
        var expiration = TimeSpan.FromMinutes(5);

        // Act
        var setResult = await _keyedCacheProvider.SetAsync(key, value, expiration);
        var getValue = await _keyedCacheProvider.GetAsync<string>(key);

        // Assert
        Assert.IsTrue(setResult);
        Assert.IsTrue(getValue.HasValue);
        Assert.AreEqual(value, getValue.Value);
    }

    /// <summary>
    ///     测试缓存过期
    /// </summary>
    [TestMethod]
    public async Task Cache_Should_Expire()
    {
        // Arrange
        var key = $"test_expire_{RandomHelper.NextString(6, true)}";
        var value = "test_expire_value";
        var expiration = TimeSpan.FromSeconds(1);

        // Act - 设置缓存
        var setResult = await _cacheProvider.SetAsync(key, value, expiration);
        Assert.IsTrue(setResult);

        // 验证缓存已设置
        var existsBeforeExpire = await _cacheProvider.ExistsAsync(key);
        Assert.IsTrue(existsBeforeExpire);

        // 等待缓存过期
        await Task.Delay(TimeSpan.FromSeconds(5));

        // 验证缓存已过期
        var getValue = await _cacheProvider.GetAsync<string>(key);
        var existsAfterExpire = await _cacheProvider.ExistsAsync(key);

        // Assert
        Assert.IsFalse(getValue.HasValue);
        Assert.IsFalse(existsAfterExpire);
    }

    /// <summary>
    /// 测试添加Redis缓存服务时传入null的配置节点
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddRedisStackExchangeCache_Should_Throw_Exception_When_RedisSection_Is_Null()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRedisStackExchangeCache(null);
    }

    /// <summary>
    /// 测试添加Redis缓存服务时传入无效的Redis配置
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void AddRedisStackExchangeCache_Should_Throw_Exception_When_RedisConfig_Is_Invalid()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Redis:InvalidConfig", "true" }
            })
            .Build();
        var services = new ServiceCollection();

        // Act
        services.AddRedisStackExchangeCache(configuration.GetSection("Redis"));
    }

    /// <summary>
    /// 测试添加Redis缓存服务时传入空的连接字符串
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void AddRedisStackExchangeCache_Should_Throw_Exception_When_ConnectionString_Is_Empty()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Redis:ConnectionString", "" }
            })
            .Build();
        var services = new ServiceCollection();

        // Act
        services.AddRedisStackExchangeCache(configuration.GetSection("Redis"));
    }

    /// <summary>
    /// 测试添加带Key的Redis缓存服务时传入null或空的serviceKey
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddKeyedRedisStackExchangeCache_Should_Throw_Exception_When_ServiceKey_Is_Null()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false)
            .Build();
        var services = new ServiceCollection();

        // Act
        services.AddKeyedRedisStackExchangeCache(null, configuration.GetSection("RedisCache:Redis"));
    }

    /// <summary>
    /// 测试添加带Key的Redis缓存服务时传入null的配置节点
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddKeyedRedisStackExchangeCache_Should_Throw_Exception_When_RedisSection_Is_Null()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddKeyedRedisStackExchangeCache("testKey", null);
    }

    /// <summary>
    /// 测试添加带Key的Redis缓存服务时传入无效的Redis配置
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddKeyedRedisStackExchangeCache_Should_Throw_Exception_When_RedisConfig_Is_Invalid()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Redis:InvalidConfig", "true" }
            })
            .Build();
        var services = new ServiceCollection();

        // Act
        services.AddKeyedRedisStackExchangeCache("testKey", configuration.GetSection("Redis"));
    }

    /// <summary>
    /// 测试添加带Key的Redis缓存服务时传入空的连接字符串
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddKeyedRedisStackExchangeCache_Should_Throw_Exception_When_ConnectionString_Is_Empty()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Redis:ConnectionString", "" }
            })
            .Build();
        var services = new ServiceCollection();

        // Act
        services.AddKeyedRedisStackExchangeCache("testKey", configuration.GetSection("Redis"));
    }
}