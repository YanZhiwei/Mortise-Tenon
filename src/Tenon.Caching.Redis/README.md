# Tenon.Caching.Redis

[![NuGet version](https://badge.fury.io/nu/Tenon.Caching.Redis.svg)](https://badge.fury.io/nu/Tenon.Caching.Redis)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Tenon.Caching.Redis 是一个 Redis 缓存抽象层实现，为 Tenon 框架提供统一的 Redis 缓存接口和基础实现。

## ✨ 功能特性

- 🚀 轻量级 Redis 缓存抽象实现
- 🔧 统一的缓存接口定义
- 💉 支持多种 Redis 客户端实现
- 🎯 完整的缓存操作支持
- 🔄 灵活的序列化选项
- 📊 可扩展的缓存提供者
- 🛡️ 内置异常处理机制

## 📦 安装方式

通过 NuGet 包管理器安装：
```bash
dotnet add package Tenon.Caching.Redis
```

## 🚀 快速入门

### 1. 实现缓存提供者

```csharp
public class CustomRedisCacheProvider : RedisCacheProvider
{
    public CustomRedisCacheProvider(
        IRedisProvider redisProvider, 
        ISerializer serializer,
        CachingOptions options) 
        : base(redisProvider, serializer, options)
    {
    }

    // 可以在这里扩展或重写基类方法
    public override T Get<T>(string key)
    {
        // 自定义实现
        return base.Get<T>(key);
    }
}
```

### 2. 注册服务

```csharp
services.AddSingleton<ICacheProvider>(sp => 
{
    var redisProvider = sp.GetRequiredService<IRedisProvider>();
    var serializer = sp.GetRequiredService<ISerializer>();
    var options = new CachingOptions();
    
    return new CustomRedisCacheProvider(
        redisProvider, 
        serializer,
        options);
});
```

### 3. 使用缓存服务

```csharp
public class CacheService
{
    private readonly ICacheProvider _cache;

    public CacheService(ICacheProvider cache)
    {
        _cache = cache;
    }

    public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan expiry)
    {
        if (_cache.TryGet(key, out T? value))
            return value;

        value = factory();
        _cache.Set(key, value, expiry);
        return value;
    }
}
```

## 📖 高级用法

### 自定义序列化

```csharp
public class CustomSerializer : ISerializer
{
    public string Serialize<T>(T value)
    {
        // 自定义序列化实现
    }

    public T Deserialize<T>(string value)
    {
        // 自定义反序列化实现
    }
}

// 注册自定义序列化器
services.AddSingleton<ISerializer, CustomSerializer>();
```

### 缓存键管理

```csharp
public class CacheKeyManager
{
    private const string Prefix = "app:";
    
    public static string BuildKey(string module, string entity, string id)
        => $"{Prefix}{module}:{entity}:{id}";
        
    public static string BuildKey<T>(string id)
        => $"{Prefix}{typeof(T).Name.ToLower()}:{id}";
}
```

## ⚙️ 接口说明

### ICacheProvider

```csharp
public interface ICacheProvider
{
    // 获取缓存值
    T Get<T>(string key);
    
    // 尝试获取缓存值
    bool TryGet<T>(string key, out T value);
    
    // 设置缓存
    void Set<T>(string key, T value, TimeSpan expiry);
    
    // 移除缓存
    void Remove(string key);
    
    // 清空缓存
    void Clear();
}
```

### RedisCacheProvider

基础 Redis 缓存提供者实现，包含：

- 基础缓存操作实现
- 序列化与反序列化处理
- 异常处理和重试机制
- 缓存键前缀管理
- 过期时间处理

## 🔨 项目依赖

- Tenon.Caching.Abstractions
- Tenon.Infra.Redis
- Tenon.Serialization.Abstractions
- Microsoft.Extensions.DependencyInjection.Abstractions

## 📝 使用注意事项

### 1. 序列化处理
- 选择合适的序列化方案
- 注意序列化性能影响
- 处理特殊类型序列化

### 2. 缓存操作
- 合理设置过期时间
- 注意并发操作处理
- 实现缓存预热机制

### 3. 最佳实践
- 统一缓存键管理
- 实现缓存监控
- 做好异常处理

## 🤝 参与贡献

欢迎参与项目贡献！请阅读我们的[贡献指南](../CONTRIBUTING.md)了解如何参与项目开发。

## 📄 开源协议

本项目采用 MIT 开源协议 - 详情请查看 [LICENSE](../LICENSE) 文件。
