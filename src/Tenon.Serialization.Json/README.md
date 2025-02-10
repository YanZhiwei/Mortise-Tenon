# Tenon.Serialization.Json

基于 System.Text.Json 的序列化实现，提供了高性能的 JSON 序列化和反序列化功能。

## 项目特点

- 基于 System.Text.Json 实现
- 支持依赖注入
- 提供日期时间转换器
- 支持命名服务注入

## 快速开始

1. 安装 NuGet 包：
```bash
dotnet add package Tenon.Serialization.Json
```

2. 添加服务：
```csharp
// 使用默认配置
services.AddSystemTextJsonSerializer();

// 或使用自定义配置
services.AddSystemTextJsonSerializer(new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
});
```

3. 使用命名服务注入：
```csharp
services.AddKeyedSystemTextJsonSerializer("custom", new JsonSerializerOptions
{
    WriteIndented = true
});
```

## 配置说明

### 默认配置

当不提供 JsonSerializerOptions 时，将使用默认的序列化配置。

### 自定义配置

你可以通过提供 JsonSerializerOptions 来自定义序列化行为：

```csharp
var options = new JsonSerializerOptions
{
    // 常用配置
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true,
    
    // 日期时间处理
    Converters =
    {
        new DateTimeConverter(),
        new DateTimeNullableConverter()
    }
};

services.AddSystemTextJsonSerializer(options);
```

## 依赖项

- Tenon.Serialization.Abstractions
- System.Text.Json
- Microsoft.Extensions.DependencyInjection.Abstractions

## 项目结构

```
Tenon.Serialization.Json/
├── Converters/           # JSON 转换器
│   ├── DateTimeConverter.cs
│   └── DateTimeNullableConverter.cs
├── Extensions/           # 扩展方法
│   └── ServiceCollectionExtension.cs
├── JsonSerializer.cs     # JSON 序列化实现
└── SystemTextJsonSerializer.cs
```

## 注意事项

1. 性能考虑：
   - System.Text.Json 相比 Newtonsoft.Json 具有更好的性能
   - 建议在性能敏感场景使用

2. 兼容性：
   - 确保目标框架支持 System.Text.Json
   - 注意日期时间格式的处理

3. 最佳实践：
   - 建议在应用程序启动时配置序列化选项
   - 使用依赖注入管理序列化器实例
   - 需要多个序列化配置时使用命名服务注入

## 使用示例

### 基本使用

```csharp
public class WeatherForecast
{
    public DateTime Date { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
}

public class WeatherService
{
    private readonly ISerializer _serializer;

    public WeatherService(ISerializer serializer)
    {
        _serializer = serializer;
    }

    public string SerializeWeather(WeatherForecast forecast)
    {
        // 序列化对象为 JSON 字符串
        return _serializer.SerializeObject(forecast);
    }

    public WeatherForecast DeserializeWeather(string json)
    {
        // 从 JSON 字符串反序列化对象
        return _serializer.DeserializeObject<WeatherForecast>(json);
    }

    public byte[] SerializeToBytes(WeatherForecast forecast)
    {
        // 序列化对象为字节数组
        return _serializer.Serialize(forecast);
    }

    public WeatherForecast DeserializeFromBytes(byte[] bytes)
    {
        // 从字节数组反序列化对象
        return _serializer.Deserialize<WeatherForecast>(bytes);
    }
}
```

### 使用命名服务

```csharp
public class MultiFormatService
{
    private readonly ISerializer _defaultSerializer;
    private readonly ISerializer _indentedSerializer;

    public MultiFormatService(
        ISerializer defaultSerializer,
        [FromKeyedServices("indented")] ISerializer indentedSerializer)
    {
        _defaultSerializer = defaultSerializer;
        _indentedSerializer = indentedSerializer;
    }

    public string SerializeCompact(WeatherForecast forecast)
    {
        // 使用默认序列化器（紧凑格式）
        return _defaultSerializer.SerializeObject(forecast);
    }

    public string SerializeIndented(WeatherForecast forecast)
    {
        // 使用缩进格式序列化器
        return _indentedSerializer.SerializeObject(forecast);
    }
}

// 在 Startup.cs 或 Program.cs 中配置
services.AddSystemTextJsonSerializer(); // 默认序列化器
services.AddKeyedSystemTextJsonSerializer("indented", new JsonSerializerOptions
{
    WriteIndented = true
});
```

### 配置示例

```csharp
public static class SerializerConfig
{
    public static IServiceCollection AddCustomJsonSerializer(this IServiceCollection services)
    {
        var options = new JsonSerializerOptions
        {
            // 常用配置
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            
            // 日期时间处理
            Converters =
            {
                new DateTimeConverter(),
                new DateTimeNullableConverter()
            },
            
            // 其他常用选项
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        services.AddSystemTextJsonSerializer(options);
        return services;
    }
}

// 使用自定义配置
services.AddCustomJsonSerializer();
```

### 输出示例

```json
// 默认输出
{
  "date":"2025-02-10T14:20:02Z",
  "temperatureC":23,
  "summary":"Warm"
}

// 缩进输出
{
  "date": "2025-02-10T14:20:02Z",
  "temperatureC": 23,
  "summary": "Warm"
}
