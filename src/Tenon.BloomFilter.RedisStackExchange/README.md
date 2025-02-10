# Tenon.BloomFilter.RedisStackExchange

[![NuGet version](https://badge.fury.io/nu/Tenon.BloomFilter.RedisStackExchange.svg)](https://badge.fury.io/nu/Tenon.BloomFilter.RedisStackExchange)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

基于 StackExchange.Redis 的高性能布隆过滤器实现，为 .NET 应用程序提供分布式布隆过滤器解决方案。

## ✨ 功能特性

- 🚀 基于 StackExchange.Redis 的高性能实现
- 🔧 支持自定义配置选项
- 💉 集成依赖注入框架
- 🎯 统一的 IBloomFilter 接口
- 🔄 支持命名服务注入
- 📊 完整的单元测试覆盖
- 🛡️ 异常重试和容错处理

## 📦 安装方式

通过 NuGet 包管理器安装：
```bash
dotnet add package Tenon.BloomFilter.RedisStackExchange
```

## 🚀 快速入门

### 1. 配置 appsettings.json

```json
{
  "RedisBloomFilter": {
    "Redis": {
      "ConnectionString": "localhost:6379,defaultDatabase=0,connectTimeout=4000,allowAdmin=true,abortConnect=false,syncTimeout=5000"
    }
  }
}
```

### 2. 注册服务

```csharp
// 使用默认配置
services.AddRedisStackExchangeBloomFilter(
    configuration.GetSection("RedisBloomFilter:Redis"),
    options => 
    {
        options.Name = "UserFilter";
        options.Capacity = 1_000_000;  // 预期存储100万个元素
        options.ErrorRate = 0.01;      // 误判率为1%
    });

// 或使用命名服务
services.AddKeyedRedisStackExchangeBloomFilter(
    "CustomFilter",
    configuration.GetSection("RedisBloomFilter:Redis"),
    options => 
    {
        // 自定义配置选项
    });
```

### 3. 使用布隆过滤器

```csharp
public class UserService
{
    private readonly IBloomFilter _filter;

    public UserService(IBloomFilter filter)
    {
        _filter = filter;
    }

    public async Task<bool> TryRegisterUserAsync(string userId)
    {
        // 检查用户ID是否已存在
        if (await _filter.ExistsAsync(userId))
            return false;  // 用户可能已存在

        // 注册新用户
        await _repository.CreateUserAsync(userId);
        
        // 添加到布隆过滤器
        await _filter.AddAsync(userId);
        
        return true;
    }
}
```

## 📖 高级用法

### 批量操作处理

```csharp
public class ProductService
{
    private readonly IBloomFilter _filter;

    public async Task ImportProductsAsync(List<string> productIds)
    {
        // 批量检查商品是否存在
        var exists = await _filter.ExistsAsync(productIds);
        
        // 过滤出不存在的商品
        var newProducts = productIds
            .Where((id, index) => !exists[index])
            .ToList();
            
        if (newProducts.Any())
        {
            // 批量导入新商品
            await _repository.ImportProductsAsync(newProducts);
            
            // 批量添加到布隆过滤器
            await _filter.AddAsync(newProducts);
        }
    }
}
```

### 多过滤器管理

```csharp
public class FilterManager
{
    private readonly IBloomFilter _userFilter;
    private readonly IBloomFilter _productFilter;

    public FilterManager(
        IBloomFilter userFilter,
        [FromKeyedServices("ProductFilter")] IBloomFilter productFilter)
    {
        _userFilter = userFilter;
        _productFilter = productFilter;
    }

    public async Task<bool> ValidateDataAsync(string userId, string productId)
    {
        var tasks = new[]
        {
            _userFilter.ExistsAsync(userId),
            _productFilter.ExistsAsync(productId)
        };

        var results = await Task.WhenAll(tasks);
        return results[0] && results[1];
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

### 布隆过滤器选项

| 配置项 | 说明 | 说明 |
|------|------|--------|
| Name | 过滤器名称 | 用于区分不同过滤器 |
| Capacity | 预期元素数量 | 影响内存使用和性能 |
| ErrorRate | 误判率 | 越小内存占用越大 |

## 🔨 项目依赖

- StackExchange.Redis
- Tenon.BloomFilter.Abstractions
- Tenon.BloomFilter.Redis
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.DependencyInjection

## 📝 使用注意事项

### 1. 性能优化
- 合理设置预期容量
- 使用批量操作减少网络请求
- 注意内存使用效率

### 2. 可靠性保证
- 实现异常重试机制
- 监控误判率变化
- 定期维护过滤器

### 3. 最佳实践
- 根据业务场景选择合适的误判率
- 预留足够的容量空间
- 实现监控和告警机制

## 🌰 应用场景示例

### 1. 注册查重

```csharp
public class RegistrationService
{
    private readonly IBloomFilter _filter;

    public async Task<bool> IsUserIdAvailableAsync(string userId)
    {
        // 布隆过滤器返回 false 表示用户ID一定不存在
        // 返回 true 表示可能存在，需要进一步查询数据库确认
        return !await _filter.ExistsAsync(userId);
    }
}
```

### 2. 缓存穿透防护

```csharp
public class CacheService
{
    private readonly IBloomFilter _filter;
    private readonly ICache _cache;

    public async Task<Product> GetProductAsync(string productId)
    {
        // 首先检查布隆过滤器
        if (!await _filter.ExistsAsync(productId))
            return null;  // 商品一定不存在
            
        // 检查缓存
        var product = await _cache.GetAsync<Product>(productId);
        if (product != null)
            return product;
            
        // 查询数据库
        product = await _repository.GetProductAsync(productId);
        if (product != null)
        {
            await _cache.SetAsync(productId, product);
            await _filter.AddAsync(productId);
        }
        
        return product;
    }
}
```

## 🤝 参与贡献

欢迎参与项目贡献！请阅读我们的[贡献指南](../CONTRIBUTING.md)了解如何参与项目开发。

## 📄 开源协议

本项目采用 MIT 开源协议 - 详情请查看 [LICENSE](../LICENSE) 文件。
