using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tenon.Infra.Redis.Configurations;
using Tenon.Infra.Redis.StackExchangeProvider.Extensions;
using Tenon.Serialization.Abstractions;

namespace Tenon.Infra.Redis.StackExchangeProvider.Tests.Extensions;

[TestClass]
public class ServiceCollectionExtensionTests
{
    private IServiceCollection _services;
    private IConfigurationSection _redisSection;

    [TestInitialize]
    public void Setup()
    {
        _services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"Redis:ConnectionString", "localhost:6379"},
                {"Redis:DatabaseId", "0"}
            })
            .Build();
        _redisSection = configuration.GetSection("Redis");
    }

    /// <summary>
    /// 测试添加Redis StackExchange Provider服务（带序列化器）
    /// </summary>
    [TestMethod]
    public void AddRedisStackExchangeProvider_WithSerializer_Should_RegisterServices()
    {
        // Act
        _services.AddRedisStackExchangeProvider<MockSerializer>(_redisSection);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        Assert.IsNotNull(serviceProvider.GetService<RedisConnection>());
        Assert.IsNotNull(serviceProvider.GetService<ISerializer>());
        Assert.IsNotNull(serviceProvider.GetService<IRedisProvider>());
    }

    /// <summary>
    /// 测试添加Redis StackExchange Provider服务（不带序列化器）
    /// </summary>
    [TestMethod]
    public void AddRedisStackExchangeProvider_WithoutSerializer_Should_RegisterServices()
    {
        // Act
        _services.AddRedisStackExchangeProvider(_redisSection);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        Assert.IsNotNull(serviceProvider.GetService<RedisConnection>());
        Assert.IsNotNull(serviceProvider.GetService<IRedisProvider>());
    }

    /// <summary>
    /// 测试添加带Key的Redis StackExchange Provider服务（带序列化器）
    /// </summary>
    [TestMethod]
    public void AddKeyedRedisStackExchangeProvider_WithSerializer_Should_RegisterServices()
    {
        // Arrange
        const string serviceKey = "test";

        // Act
        _services.AddKeyedRedisStackExchangeProvider<MockSerializer>(serviceKey, _redisSection);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        Assert.IsNotNull(serviceProvider.GetKeyedService<RedisConnection>(serviceKey));
        Assert.IsNotNull(serviceProvider.GetKeyedService<ISerializer>(serviceKey));
        Assert.IsNotNull(serviceProvider.GetKeyedService<IRedisProvider>(serviceKey));
    }

    /// <summary>
    /// 测试添加带Key的Redis StackExchange Provider服务（不带序列化器）
    /// </summary>
    [TestMethod]
    public void AddKeyedRedisStackExchangeProvider_WithoutSerializer_Should_RegisterServices()
    {
        // Arrange
        const string serviceKey = "test";

        // Act
        _services.AddKeyedRedisStackExchangeProvider(serviceKey, _redisSection);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        Assert.IsNotNull(serviceProvider.GetKeyedService<RedisConnection>(serviceKey));
        Assert.IsNotNull(serviceProvider.GetKeyedService<IRedisProvider>(serviceKey));
    }

    /// <summary>
    /// 测试Redis配置为空时抛出异常
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(NullReferenceException))]
    public void AddRedisStackExchangeProvider_WithNullConfig_Should_ThrowException()
    {
        // Arrange
        var emptyConfiguration = new ConfigurationBuilder().Build();
        var emptySection = emptyConfiguration.GetSection("Redis");

        // Act
        _services.AddRedisStackExchangeProvider(emptySection);
    }

    /// <summary>
    /// 测试服务Key为空时抛出异常
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddKeyedRedisStackExchangeProvider_WithNullKey_Should_ThrowException()
    {
        // Act
        _services.AddKeyedRedisStackExchangeProvider(null, _redisSection);
    }
}

/// <summary>
/// 模拟序列化器
/// </summary>
public class MockSerializer : ISerializer
{
    public string Serialize<T>(T obj)
    {
        return string.Empty;
    }

    public T? Deserialize<T>(string str)
    {
        return default;
    }
}
