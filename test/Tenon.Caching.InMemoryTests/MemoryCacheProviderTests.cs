using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tenon.Caching.Abstractions;

namespace Tenon.Caching.InMemoryTests;

[TestClass]
public class MemoryCacheProviderTests
{
    private ICacheProvider _cacheProvider;
    private IServiceProvider _serviceProvider;

    [TestInitialize]
    public void Initialize()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICacheProvider, InMemory.MemoryCacheProvider>();
        _serviceProvider = services.BuildServiceProvider();
        _cacheProvider = _serviceProvider.GetRequiredService<ICacheProvider>();
    }

    /// <summary>
    /// 测试设置和获取缓存值
    /// </summary>
    [TestMethod]
    public void Set_And_Get_Should_Work()
    {
        // Arrange
        var key = "test_key";
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
    /// 测试异步设置和获取缓存值
    /// </summary>
    [TestMethod]
    public async Task SetAsync_And_GetAsync_Should_Work()
    {
        // Arrange
        var key = "test_key_async";
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
    /// 测试删除缓存值
    /// </summary>
    [TestMethod]
    public async Task Remove_Should_Work()
    {
        // Arrange
        var key = "test_key_remove";
        var value = "test_value_remove";
        var expiration = TimeSpan.FromMinutes(5);
        _cacheProvider.Set(key, value, expiration);

        // 等待一段时间确保缓存项已经设置成功
        await Task.Delay(100);
        var getValue = _cacheProvider.Get<string>(key);
        Assert.IsTrue(getValue.HasValue);
        Assert.AreEqual(value, getValue.Value);

        // Act
        var removeResult = _cacheProvider.Remove(key);

        // 等待一段时间确保缓存项已经被删除
        await Task.Delay(100);
        getValue = _cacheProvider.Get<string>(key);

        // Assert
        Assert.IsTrue(removeResult);
        Assert.IsTrue(getValue.IsNull);
    }

    /// <summary>
    /// 测试异步删除缓存值
    /// </summary>
    [TestMethod]
    public async Task RemoveAsync_Should_Work()
    {
        // Arrange
        var key = "test_key_remove_async";
        var value = "test_value_remove_async";
        var expiration = TimeSpan.FromMinutes(5);
        await _cacheProvider.SetAsync(key, value, expiration);

        // 等待一段时间确保缓存项已经设置成功
        await Task.Delay(100);
        var getValue = await _cacheProvider.GetAsync<string>(key);
        Assert.IsTrue(getValue.HasValue);
        Assert.AreEqual(value, getValue.Value);

        // Act
        var removeResult = await _cacheProvider.RemoveAsync(key);

        // 等待一段时间确保缓存项已经被删除
        await Task.Delay(100);
        getValue = await _cacheProvider.GetAsync<string>(key);

        // Assert
        Assert.IsTrue(removeResult);
        Assert.IsTrue(getValue.IsNull);
    }

    /// <summary>
    /// 测试检查缓存键是否存在
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
    /// 测试异步检查缓存键是否存在
    /// </summary>
    [TestMethod]
    public async Task ExistsAsync_Should_Work()
    {
        // Arrange
        var key = "test_key_exists_async";
        var value = "test_value_exists_async";
        var expiration = TimeSpan.FromMinutes(5);
        await _cacheProvider.SetAsync(key, value, expiration);

        // Act
        var existsResult = await _cacheProvider.ExistsAsync(key);

        // Assert
        Assert.IsTrue(existsResult);
    }

    /// <summary>
    /// 测试批量删除缓存值
    /// </summary>
    [TestMethod]
    public void RemoveAll_Should_Work()
    {
        // Arrange
        var keys = new[] { "key1", "key2", "key3" };
        var value = "test_value";
        var expiration = TimeSpan.FromMinutes(5);
        foreach (var key in keys)
        {
            _cacheProvider.Set(key, value, expiration);
        }

        // Act
        var removeCount = _cacheProvider.RemoveAll(keys);

        // Assert
        Assert.AreEqual(keys.Length, removeCount);
        foreach (var key in keys)
        {
            Assert.IsFalse(_cacheProvider.Exists(key));
        }
    }

    /// <summary>
    /// 测试异步批量删除缓存值
    /// </summary>
    [TestMethod]
    public async Task RemoveAllAsync_Should_Work()
    {
        // Arrange
        var keys = new[] { "key1_async", "key2_async", "key3_async" };
        var value = "test_value";
        var expiration = TimeSpan.FromMinutes(5);
        foreach (var key in keys)
        {
            await _cacheProvider.SetAsync(key, value, expiration);
        }

        // Act
        var removeCount = await _cacheProvider.RemoveAllAsync(keys);

        // Assert
        Assert.AreEqual(keys.Length, removeCount);
        foreach (var key in keys)
        {
            Assert.IsFalse(await _cacheProvider.ExistsAsync(key));
        }
    }

    /// <summary>
    /// 测试批量设置缓存过期时间
    /// </summary>
    [TestMethod]
    public async Task KeysExpireAsync_Should_Work()
    {
        // Arrange
        var keys = new[] { "expire_key1", "expire_key2" };
        var value = "test_value";
        var initialExpiration = TimeSpan.FromMinutes(5);
        foreach (var key in keys)
        {
            await _cacheProvider.SetAsync(key, value, initialExpiration);
        }

        // Act
        var newExpiration = TimeSpan.FromMinutes(10);
        await _cacheProvider.KeysExpireAsync(keys, newExpiration);

        // Assert
        foreach (var key in keys)
        {
            Assert.IsTrue(await _cacheProvider.ExistsAsync(key));
        }
    }

    [TestCleanup]
    public void Cleanup()
    {
        (_cacheProvider as IDisposable)?.Dispose();
    }
}
