using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Tenon.Serialization.Abstractions;
using Tenon.Serialization.Json.Extensions;

namespace Tenon.Serialization.Json.Tests;

[TestClass]
public class JsonSerializerTests
{
    private ISerializer _jsonSerializer;
    private IServiceProvider _serviceProvider;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddSystemTextJsonSerializer();
        _serviceProvider = services.BuildServiceProvider();
        _jsonSerializer = _serviceProvider.GetRequiredService<ISerializer>();
    }

    /// <summary>
    /// 测试基本类型序列化和反序列化
    /// </summary>
    [TestMethod]
    public void SerializeAndDeserialize_BasicType_ShouldWork()
    {
        // Arrange
        var originalValue = 42;

        // Act
        var serialized = _jsonSerializer.Serialize(originalValue);
        var deserialized = _jsonSerializer.Deserialize<int>(serialized);

        // Assert
        Assert.AreEqual(originalValue, deserialized);
    }

    /// <summary>
    /// 测试复杂对象序列化和反序列化
    /// </summary>
    [TestMethod]
    public void SerializeAndDeserialize_ComplexObject_ShouldWork()
    {
        // Arrange
        var person = new TestPerson
        {
            Name = "张三",
            Age = 30,
            Email = "zhangsan@example.com"
        };

        // Act
        var serialized = _jsonSerializer.SerializeObject(person);
        var deserialized = _jsonSerializer.DeserializeObject<TestPerson>(serialized);

        // Assert
        Assert.AreEqual(person.Name, deserialized.Name);
        Assert.AreEqual(person.Age, deserialized.Age);
        Assert.AreEqual(person.Email, deserialized.Email);
    }

    /// <summary>
    /// 测试日期时间序列化和反序列化
    /// </summary>
    [TestMethod]
    public void SerializeAndDeserialize_DateTime_ShouldWork()
    {
        // Arrange
        var dateTime = new DateTime(2023, 12, 12, 10, 30, 0, DateTimeKind.Utc);
        var testObject = new TestDateTimeObject { CreatedAt = dateTime };

        // Act
        var serialized = _jsonSerializer.SerializeObject(testObject);
        var deserialized = _jsonSerializer.DeserializeObject<TestDateTimeObject>(serialized);

        // Assert
        Assert.AreEqual(dateTime, deserialized.CreatedAt);
    }

    /// <summary>
    /// 测试驼峰命名策略
    /// </summary>
    [TestMethod]
    public void Serialize_WithCamelCase_ShouldWork()
    {
        // Arrange
        var testObject = new TestNamingObject
        {
            FirstName = "张",
            LastName = "三"
        };

        // Act
        var serialized = _jsonSerializer.SerializeObject(testObject);

        // Assert
        Assert.IsTrue(serialized.Contains("firstName"));
        Assert.IsTrue(serialized.Contains("lastName"));
    }

    /// <summary>
    /// 测试使用自定义选项的序列化器
    /// </summary>
    [TestMethod]
    public void SerializeWithCustomOptions_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null // 禁用驼峰命名
        };
        services.AddSystemTextJsonSerializer(options);
        var serviceProvider = services.BuildServiceProvider();
        var customSerializer = serviceProvider.GetRequiredService<ISerializer>();

        var testObject = new TestNamingObject
        {
            FirstName = "张",
            LastName = "三"
        };

        // Act
        var serialized = customSerializer.SerializeObject(testObject);

        // Assert
        Assert.IsTrue(serialized.Contains("FirstName")); // 应该使用Pascal命名
        Assert.IsTrue(serialized.Contains("LastName"));
    }

    /// <summary>
    /// 测试命名服务的序列化器
    /// </summary>
    [TestMethod]
    public void SerializeWithNamedService_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddKeyedSystemTextJsonSerializer("custom");
        var serviceProvider = services.BuildServiceProvider();
        var namedSerializer = serviceProvider.GetKeyedService<ISerializer>("custom");

        var testObject = new TestPerson
        {
            Name = "张三",
            Age = 30,
            Email = "zhangsan@example.com"
        };

        // Act
        var serialized = namedSerializer.SerializeObject(testObject);
        var deserialized = namedSerializer.DeserializeObject<TestPerson>(serialized);

        // Assert
        Assert.AreEqual(testObject.Name, deserialized.Name);
        Assert.AreEqual(testObject.Age, deserialized.Age);
        Assert.AreEqual(testObject.Email, deserialized.Email);
    }
}

public class TestPerson
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
}

public class TestDateTimeObject
{
    public DateTime CreatedAt { get; set; }
}

public class TestNamingObject
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}