# Tenon.Caching.Abstractions

[![NuGet version](https://badge.fury.io/nu/Tenon.Caching.Abstractions.svg)](https://badge.fury.io/nu/Tenon.Caching.Abstractions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Tenon.Caching.Abstractions 提供了统一的缓存抽象接口定义，是 Tenon 框架缓存功能的核心基础。通过抽象接口设计，实现了缓存提供者的可插拔性和一致性。

## ✨ 设计优势

- 🎯 **统一抽象**：提供统一的 `ICacheProvider` 接口，确保不同缓存实现的一致性
- 🔌 **可插拔性**：支持多种缓存实现无缝切换，无需修改业务代码
- 💡 **简洁接口**：精心设计的 API 接口，易于使用和扩展
- 🛡️ **类型安全**：泛型设计确保类型安全，避免运行时类型错误
- 🔄 **异步支持**：全面支持异步操作，提升性能
- 📦 **批量操作**：支持批量缓存操作，提高效率
- ⚡ **高性能**：优化的缓存值包装器，最小化性能开销

## 📦 安装方式

通过 NuGet 包管理器安装：
```bash
dotnet add package Tenon.Caching.Abstractions
```

## 🚀 核心接口

### ICacheProvider

提供统一的缓存操作接口：

```csharp
public interface ICacheProvider
{
    // 设置缓存
    bool Set<T>(string cacheKey, T cacheValue, TimeSpan expiration);
    Task<bool> SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration);
    
    // 获取缓存
    CacheValue<T> Get<T>(string cacheKey);
    Task<CacheValue<T>> GetAsync<T>(string cacheKey);
    
    // 删除缓存
    bool Remove(string cacheKey);
    Task<bool> RemoveAsync(string cacheKey);
    
    // 检查缓存是否存在
    bool Exists(string cacheKey);
    Task<bool> ExistsAsync(string cacheKey);
    
    // 批量操作
    long RemoveAll(IEnumerable<string> cacheKeys);
    Task<long> RemoveAllAsync(IEnumerable<string> cacheKeys);
    
    // 过期设置
    Task KeysExpireAsync(IEnumerable<string> cacheKeys);
    Task KeysExpireAsync(IEnumerable<string> cacheKeys, TimeSpan expiration);
}
```

### CacheValue<T>

优化的缓存值包装器：

```csharp
public readonly struct CacheValue<T>
{
    public bool HasValue { get; }
    public bool IsNull { get; }
    public T Value { get; }
    
    public static CacheValue<T> Null { get; }
    public static CacheValue<T> NoValue { get; }
}
```

## 📚 缓存实现

Tenon 框架提供了多种缓存实现，都基于此抽象接口：

### 1. 内存缓存
[Tenon.Caching.InMemory](../Tenon.Caching.InMemory/README.md)
- 基于 System.Runtime.Caching
- 适用于单机部署场景
- 高性能、低延迟

### 2. Redis 缓存
[Tenon.Caching.Redis](../Tenon.Caching.Redis/README.md)
- Redis 缓存抽象实现
- 支持多种 Redis 客户端
- 分布式缓存基础

### 3. StackExchange.Redis 实现
[Tenon.Caching.RedisStackExchange](../Tenon.Caching.RedisStackExchange/README.md)
- 基于 StackExchange.Redis
- 企业级分布式缓存方案
- 高性能、高可用

### 4. Castle 拦截器
[Tenon.Caching.Interceptor.Castle](../Tenon.Caching.Interceptor.Castle/README.md)
- AOP 缓存实现
- Cache-Aside 模式
- 延时双删策略

## 🎯 使用示例

### 1. 基础用法

```csharp
public class UserService
{
    private readonly ICacheProvider _cache;
    
    public UserService(ICacheProvider cache)
    {
        _cache = cache;
    }
    
    public async Task<User> GetUserAsync(int userId)
    {
        var cacheKey = $"user:{userId}";
        
        // 尝试获取缓存
        var cacheValue = await _cache.GetAsync<User>(cacheKey);
        if (cacheValue.HasValue)
            return cacheValue.Value;
            
        // 从数据源获取
        var user = await _repository.GetUserAsync(userId);
        
        // 设置缓存
        await _cache.SetAsync(cacheKey, user, TimeSpan.FromHours(1));
        
        return user;
    }
}
```

### 2. 批量操作

```csharp
public class ProductService
{
    private readonly ICacheProvider _cache;
    
    public async Task UpdateProductsAsync(List<Product> products)
    {
        // 更新数据
        await _repository.UpdateProductsAsync(products);
        
        // 批量清除缓存
        var cacheKeys = products.Select(p => $"product:{p.Id}");
        await _cache.RemoveAllAsync(cacheKeys);
    }
}
```

## ⚙️ 最佳实践

### 1. 缓存键设计
```csharp
public static class CacheKeys
{
    private const string Prefix = "app:";
    
    public static string GetUserKey(int userId) 
        => $"{Prefix}user:{userId}";
        
    public static string GetProductKey(int productId)
        => $"{Prefix}product:{productId}";
}
```

### 2. 异常处理
```csharp
public async Task<User> GetUserWithRetryAsync(int userId)
{
    try
    {
        var cacheValue = await _cache.GetAsync<User>(
            CacheKeys.GetUserKey(userId));
            
        return cacheValue.HasValue 
            ? cacheValue.Value 
            : await GetFromSourceAsync(userId);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "缓存操作失败");
        return await GetFromSourceAsync(userId);
    }
}
```

## 🔨 项目依赖

- Microsoft.Extensions.DependencyInjection.Abstractions
- System.Threading.Tasks

## 📝 使用注意事项

### 1. 接口设计
- 保持接口简单清晰
- 支持同步和异步操作
- 提供批量操作能力

### 2. 缓存策略
- 合理设置过期时间
- 实现缓存预热机制
- 考虑缓存穿透问题

### 3. 性能优化
- 使用批量操作减少网络请求
- 合理使用异步操作
- 注意缓存大小控制

## 🤝 参与贡献

欢迎参与项目贡献！请阅读我们的[贡献指南](../CONTRIBUTING.md)了解如何参与项目开发。

## 📄 开源协议

本项目采用 MIT 开源协议 - 详情请查看 [LICENSE](../LICENSE) 文件。
