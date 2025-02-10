# Tenon.Caching.InMemory

[![NuGet version](https://badge.fury.io/nu/Tenon.Caching.InMemory.svg)](https://badge.fury.io/nu/Tenon.Caching.InMemory)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

基于 System.Runtime.Caching.MemoryCache 的高性能内存缓存实现，为 .NET 应用程序提供简单且灵活的缓存操作接口。

## ✨ 功能特性

- 🚀 基于 MemoryCache 的高性能实现
- 🔧 支持自定义缓存配置
- 💉 集成依赖注入框架
- 🎯 统一的 ICacheProvider 接口
- 🔄 自动过期缓存清理
- 📊 可配置内存使用限制
- 🛡️ 保证线程安全

## 📦 安装方式

通过 NuGet 包管理器安装：
```bash
dotnet add package Tenon.Caching.InMemory
```

## 🚀 快速入门

### 1. 注册服务
在 `Startup.cs` 或 `Program.cs` 中配置服务：

```csharp
// 使用默认配置
services.AddInMemoryCache();

// 或使用自定义配置
services.AddInMemoryCache(options =>
{
    // 设置最大内存限制为 1GB
    options.CacheMemoryLimitMegabytes = 1024;    
    // 使用最多 50% 的物理内存
    options.PhysicalMemoryLimitPercentage = 50;  
    // 每 5 分钟清理过期缓存
    options.PollingInterval = TimeSpan.FromMinutes(5); 
});
```

### 2. 使用缓存服务

```csharp
public class WeatherService
{
    private readonly ICacheProvider _cache;

    public WeatherService(ICacheProvider cache)
    {
        _cache = cache;
    }

    public async Task<WeatherForecast> GetForecastAsync(string city)
    {
        var cacheKey = $"weather:{city}";
        
        // 尝试从缓存获取数据
        if (_cache.TryGet(cacheKey, out WeatherForecast? forecast))
            return forecast;

        // 缓存未命中，从数据源获取
        forecast = await GetForecastFromApiAsync(city);
        
        // 存入缓存，设置 30 分钟过期
        _cache.Set(cacheKey, forecast, TimeSpan.FromMinutes(30));
        
        return forecast;
    }
}
```

## 📖 高级用法

### 自定义缓存配置

```csharp
services.AddInMemoryCache(options =>
{
    // 基础配置
    options.CacheName = "CustomCache";
    options.CacheMemoryLimitMegabytes = 2048;
    
    // 内存限制
    options.PhysicalMemoryLimitPercentage = 75;
    
    // 清理配置
    options.PollingInterval = TimeSpan.FromMinutes(10);
});
```

### 缓存操作示例

```csharp
public class CacheExample
{
    private readonly ICacheProvider _cache;

    public CacheExample(ICacheProvider cache)
    {
        _cache = cache;
    }

    public void CacheOperations()
    {
        // 设置字符串缓存
        _cache.Set("key1", "value1", TimeSpan.FromHours(1));

        // 获取缓存数据
        if (_cache.TryGet("key1", out string? value))
        {
            Console.WriteLine($"缓存命中: {value}");
        }

        // 删除缓存
        _cache.Remove("key1");

        // 缓存复杂对象
        var user = new User { Id = 1, Name = "张三" };
        _cache.Set($"user:{user.Id}", user, TimeSpan.FromMinutes(30));
    }
}
```

## ⚙️ 配置选项说明

| 配置项 | 说明 | 默认值 |
|------|------|--------|
| CacheName | 缓存实例名称 | MemoryCacheProvider |
| CacheMemoryLimitMegabytes | 最大内存限制（MB） | 不限制 |
| PhysicalMemoryLimitPercentage | 物理内存使用限制百分比 | 不限制 |
| PollingInterval | 过期缓存清理间隔 | 2分钟 |

## 🔨 项目依赖

- System.Runtime.Caching
- Tenon.Caching.Abstractions
- Microsoft.Extensions.DependencyInjection.Abstractions

## 📁 项目结构

```
Tenon.Caching.InMemory/
├── Configurations/
│   └── InMemoryCachingOptions.cs    # 缓存配置选项
├── Extensions/
│   ├── CachingOptionsExtension.cs    # 缓存选项扩展
│   └── ServiceCollectionExtension.cs # 服务注册扩展
├── MemoryCacheProvider.cs           # 内存缓存实现
└── Tenon.Caching.InMemory.csproj    # 项目文件
```

## 📝 使用注意事项

### 1. 内存管理
- 根据应用程序需求合理设置内存限制
- 为缓存项设置合适的过期时间
- 定期监控缓存命中率和内存使用情况

### 2. 性能优化
- 合理配置缓存清理间隔
- 避免缓存过大的对象
- 使用合适的缓存策略

### 3. 最佳实践
- 采用统一的缓存键命名规范
- 实现缓存预热机制
- 添加必要的缓存监控和日志记录

## 🤝 参与贡献

欢迎参与项目贡献！请阅读我们的[贡献指南](../CONTRIBUTING.md)了解如何参与项目开发。

## 📄 开源协议

本项目采用 MIT 开源协议 - 详情请查看 [LICENSE](../LICENSE) 文件。
