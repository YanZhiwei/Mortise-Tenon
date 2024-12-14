using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tenon.Infra.Redis.Configurations;

namespace Tenon.Infra.Redis.StackExchangeProvider.Tests;

[TestClass]
public class RedisConnectionTests
{
    private RedisOptions _redisOptions;
    private Mock<IOptionsMonitor<RedisOptions>> _optionsMonitorMock;

    [TestInitialize]
    public void Setup()
    {
        _redisOptions = new RedisOptions
        {
            ConnectionString = "localhost:6379",
            DatabaseId = 0
        };

        _optionsMonitorMock = new Mock<IOptionsMonitor<RedisOptions>>();
        _optionsMonitorMock.Setup(x => x.CurrentValue).Returns(_redisOptions);
    }

    /// <summary>
    /// 测试使用RedisOptions构造RedisConnection
    /// </summary>
    [TestMethod]
    public void Constructor_WithRedisOptions_Should_CreateInstance()
    {
        // Act
        var connection = new RedisConnection(_redisOptions);

        // Assert
        Assert.IsNotNull(connection);
    }

    /// <summary>
    /// 测试使用IOptionsMonitor构造RedisConnection
    /// </summary>
    [TestMethod]
    public void Constructor_WithOptionsMonitor_Should_CreateInstance()
    {
        // Act
        var connection = new RedisConnection(_optionsMonitorMock.Object);

        // Assert
        Assert.IsNotNull(connection);
    }

    /// <summary>
    /// 测试RedisOptions为空时抛出异常
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_WithNullRedisOptions_Should_ThrowException()
    {
        // Act
        _ = new RedisConnection((RedisOptions)null);
    }

    /// <summary>
    /// 测试IOptionsMonitor为空时抛出异常
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_WithNullOptionsMonitor_Should_ThrowException()
    {
        // Act
        _ = new RedisConnection((IOptionsMonitor<RedisOptions>)null);
    }

    /// <summary>
    /// 测试获取数据库实例
    /// </summary>
    [TestMethod]
    public void GetDatabase_Should_ReturnInstance()
    {
        // Arrange
        var connection = new RedisConnection(_redisOptions);

        // Act
        var database = connection.GetDatabase();

        // Assert
        Assert.IsNotNull(database);
    }

    /// <summary>
    /// 测试获取服务器实例
    /// </summary>
    [TestMethod]
    public void GetServers_Should_ReturnInstances()
    {
        // Arrange
        var connection = new RedisConnection(_redisOptions);

        // Act
        var servers = connection.GetServers();

        // Assert
        Assert.IsNotNull(servers);
    }
}
