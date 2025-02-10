# Tenon.Caching.RedisStackExchange

[![NuGet version](https://badge.fury.io/nu/Tenon.Caching.RedisStackExchange.svg)](https://badge.fury.io/nu/Tenon.Caching.RedisStackExchange)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

基于 StackExchange.Redis 的高性能 Redis 缓存实现，为 .NET 应用程序提供分布式缓存解决方案。

## ✨ 功能特性

- 🚀 基于 StackExchange.Redis 的高性能实现
- 🔧 支持自定义缓存配置
- 💉 集成依赖注入框架
- 🎯 统一的 ICacheProvider 接口
- 🔄 支持命名服务注入
- 📊 完整的单元测试覆盖
- 🛡️ 异常重试和容错处理

## 📦 安装方式

通过 NuGet 包管理器安装：
```bash
dotnet add package Tenon.Caching.RedisStackExchange
```

## 🚀 快速入门

### 1. 配置 appsettings.json

```json
{
  "RedisCache": {
    "MaxRandomSecond": 5,
    "Redis": {
      "ConnectionString": "localhost:6379,defaultDatabase=0,connectTimeout=4000,allowAdmin=true,abortConnect=false,syncTimeout=5000"
    }
  }
}
```

### 2. 注册服务

```csharp
// 使用默认配置
services.AddRedisStackExchangeCache(
    configuration.GetSection("RedisCache:Redis"));

// 或使用命名服务
services.AddKeyedRedisStackExchangeCache(
    "CustomCache",
    configuration.GetSection("RedisCache:Redis"),
    options => 
    {
        // 自定义缓存选项
    });
```

### 3. 使用缓存服务

```csharp
public class ProductService
{
    private readonly ICacheProvider _cache;

    public ProductService(ICacheProvider cache)
    {
        _cache = cache;
    }

    public async Task<Product> GetProductAsync(int id)
    {
        var cacheKey = $"product:{id}";
        
        // 尝试从缓存获取数据
        if (_cache.TryGet(cacheKey, out Product? product))
            return product;

        // 缓存未命中，从数据库获取
        product = await GetProductFromDbAsync(id);
        
        // 存入缓存，设置 1 小时过期
        _cache.Set(cacheKey, product, TimeSpan.FromHours(1));
        
        return product;
    }
}
```

## 📖 高级用法

### 使用命名服务

```csharp
public class MultiCacheService
{
    private readonly ICacheProvider _defaultCache;
    private readonly ICacheProvider _customCache;

    public MultiCacheService(
        ICacheProvider defaultCache,
        [FromKeyedServices("CustomCache")] ICacheProvider customCache)
    {
        _defaultCache = defaultCache;
        _customCache = customCache;
    }

    public async Task<Product> GetProductWithBackupAsync(int id)
    {
        // 先从主缓存获取
        var cacheKey = $"product:{id}";
        if (_defaultCache.TryGet(cacheKey, out Product? product))
            return product;

        // 从备份缓存获取
        if (_customCache.TryGet(cacheKey, out product))
        {
            // 同步到主缓存
            _defaultCache.Set(cacheKey, product, TimeSpan.FromHours(1));
            return product;
        }

        // 都未命中，从数据源获取
        return await GetProductFromSourceAsync(id);
    }
}
```

### 批量操作

```csharp
public class BulkOperationExample
{
    private readonly ICacheProvider _cache;

    public BulkOperationExample(ICacheProvider cache)
    {
        _cache = cache;
    }

    public void BatchSetProducts(List<Product> products)
    {
        foreach (var product in products)
        {
            _cache.Set(
                $"product:{product.Id}", 
                product, 
                TimeSpan.FromHours(1));
        }
    }

    public List<Product> GetProductsByIds(List<int> ids)
    {
        return ids
            .Select(id => _cache.TryGet($"product:{id}", out Product? product) 
                ? product 
                : null)
            .Where(p => p != null)
            .ToList();
    }
}
```

## ⚙️ 配置选项说明

### Redis 连接配置

| 配置项 | 说明 | 示例值 |
|------|------|--------|
| ConnectionString | Redis 连接字符串 | localhost:6379 |
| DefaultDatabase | 默认数据库编号 | 0 |
| ConnectTimeout | 连接超时时间(ms) | 4000 |
| SyncTimeout | 同步操作超时时间(ms) | 5000 |
| AllowAdmin | 允许管理员操作 | true |
| AbortConnect | 连接失败时终止 | false |

### 缓存选项

| 配置项 | 说明 | 默认值 |
|------|------|--------|
| MaxRandomSecond | 过期时间随机偏移最大秒数 | 5 |
| RetryCount | 操作重试次数 | 3 |
| RetryInterval | 重试间隔(ms) | 1000 |

## 🔨 项目依赖

- StackExchange.Redis
- Tenon.Caching.Abstractions
- Tenon.Caching.Redis
- Tenon.Serialization.Json
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.DependencyInjection

## 📝 使用注意事项

### 1. 连接管理
- 合理配置连接池大小
- 设置适当的超时时间
- 启用连接复用

### 2. 性能优化
- 使用批量操作减少网络往返
- 合理设置数据序列化格式
- 避免存储过大的数据

### 3. 最佳实践
- 实现缓存穿透保护
- 使用分布式锁避免缓存击穿
- 采用合理的缓存更新策略

## ✅ 单元测试

项目包含完整的单元测试：`Tenon.Caching.RedisStackExchangeTests`

```csharp
[TestClass]
public class RedisStackExchangeCacheTests
{
    [TestMethod]
    public void Set_And_Get_Should_Work()
    {
        // 测试代码...
    }

    [TestMethod]
    public void Remove_Should_Work()
    {
        // 测试代码...
    }
}
```

## 🤝 参与贡献

欢迎参与项目贡献！请阅读我们的[贡献指南](../CONTRIBUTING.md)了解如何参与项目开发。

## 📄 开源协议

本项目采用 MIT 开源协议 - 详情请查看 [LICENSE](../LICENSE) 文件。
