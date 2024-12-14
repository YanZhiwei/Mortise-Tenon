using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tenon.Infra.Redis.Configurations;
using Tenon.Infra.Redis.StackExchangeProvider.Tests.Extensions;

namespace Tenon.Infra.Redis.StackExchangeProvider.Tests;

[TestClass]
public class StackExchangeProviderTests
{
    private RedisOptions _redisOptions;
    private RedisConnection _redisConnection;
    private MockSerializer _serializer;

    [TestInitialize]
    public void Setup()
    {
        _redisOptions = new RedisOptions
        {
            ConnectionString = "localhost:6379",
            DatabaseId = 0
        };
        _redisConnection = new RedisConnection(_redisOptions);
        _serializer = new MockSerializer();
    }

    /// <summary>
    /// 测试使用RedisConnection和序列化器构造StackExchangeProvider
    /// </summary>
    [TestMethod]
    public void Constructor_WithConnectionAndSerializer_Should_CreateInstance()
    {
        // Act
        var provider = new StackExchangeProvider(_redisConnection, _serializer);

        // Assert
        Assert.IsNotNull(provider);
    }

    /// <summary>
    /// 测试使用RedisOptions和序列化器构造StackExchangeProvider
    /// </summary>
    [TestMethod]
    public void Constructor_WithOptionsAndSerializer_Should_CreateInstance()
    {
        // Act
        var provider = new StackExchangeProvider(_redisOptions, _serializer);

        // Assert
        Assert.IsNotNull(provider);
    }

    /// <summary>
    /// 测试使用RedisOptions构造StackExchangeProvider
    /// </summary>
    [TestMethod]
    public void Constructor_WithOptions_Should_CreateInstance()
    {
        // Act
        var provider = new StackExchangeProvider(_redisOptions);

        // Assert
        Assert.IsNotNull(provider);
    }

    /// <summary>
    /// 测试使用RedisConnection构造StackExchangeProvider
    /// </summary>
    [TestMethod]
    public void Constructor_WithConnection_Should_CreateInstance()
    {
        // Act
        var provider = new StackExchangeProvider(_redisConnection);

        // Assert
        Assert.IsNotNull(provider);
    }

    /// <summary>
    /// 测试RedisConnection为空时抛出异常
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_WithNullConnection_Should_ThrowException()
    {
        // Act
        _ = new StackExchangeProvider((RedisConnection)null, _serializer);
    }

    /// <summary>
    /// 测试检查缓存键为空时抛出异常
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void CheckCacheKey_WithNullKey_Should_ThrowException()
    {
        // Arrange
        var provider = new StackExchangeProvider(_redisConnection);

        // Act
        provider.StringSet(null, "value");
    }
}
