# Tenon.BloomFilter.Abstractions

[![NuGet version](https://badge.fury.io/nu/Tenon.BloomFilter.Abstractions.svg)](https://badge.fury.io/nu/Tenon.BloomFilter.Abstractions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Tenon.BloomFilter.Abstractions 提供了统一的布隆过滤器抽象接口定义，是 Tenon 框架布隆过滤器功能的核心基础。通过抽象接口设计，实现了布隆过滤器的可插拔性和一致性。

## ✨ 设计优势

- 🎯 **统一抽象**：提供统一的 `IBloomFilter` 接口，确保不同实现的一致性
- 🔌 **可插拔性**：支持多种存储介质实现无缝切换
- 💡 **简洁接口**：精心设计的 API 接口，易于使用和扩展
- 🔄 **异步支持**：全面支持异步操作，提升性能
- 📦 **批量操作**：支持批量添加和检查，提高效率
- ⚡ **高性能**：优化的布隆过滤器实现，最小化误判率
- 🛠️ **可配置性**：灵活的配置选项，满足不同场景需求

## 📦 安装方式

通过 NuGet 包管理器安装：
```bash
dotnet add package Tenon.BloomFilter.Abstractions
```

## 🚀 核心接口

### IBloomFilter

提供统一的布隆过滤器操作接口：

```csharp
public interface IBloomFilter
{
    // 获取配置选项
    BloomFilterOptions Options { get; }
    
    // 初始化过滤器
    Task<bool> InitAsync();
    bool Init();
    
    // 添加元素
    Task<bool> AddAsync(string value);
    bool Add(string value);
    
    // 批量添加元素
    Task<bool[]> AddAsync(IEnumerable<string> values);
    bool[] Add(IEnumerable<string> values);
    
    // 检查元素是否存在
    Task<bool> ExistsAsync(string value);
    bool Exists(string value);
    
    // 批量检查元素
    Task<bool[]> ExistsAsync(IEnumerable<string> values);
    
    // 检查过滤器是否存在
    Task<bool> ExistsAsync();
    bool Exists();
}
```

### BloomFilterOptions

布隆过滤器配置选项：

```csharp
public class BloomFilterOptions
{
    // 过滤器名称
    public string Name { get; set; }
    
    // 误判率
    public double ErrorRate { get; set; }
    
    // 预计元素数量
    public int Capacity { get; set; }
    
    // 命名服务键
    public string? KeyedServiceKey { get; set; }
}
```

## 📚 布隆过滤器实现

Tenon 框架提供了多种布隆过滤器实现：

### 1. Redis 实现
[Tenon.BloomFilter.Redis](../../Tenon.BloomFilter.Redis/README.md)
- Redis 布隆过滤器抽象实现
- 支持多种 Redis 客户端
- 分布式布隆过滤器基础

### 2. StackExchange.Redis 实现
[Tenon.BloomFilter.RedisStackExchange](../../Tenon.BloomFilter.RedisStackExchange/README.md)
- 基于 StackExchange.Redis
- 企业级分布式布隆过滤器方案
- 高性能、高可用

## 🎯 使用示例

### 1. 基础用法

```csharp
public class UserService
{
    private readonly IBloomFilter _filter;
    
    public UserService(IBloomFilter filter)
    {
        _filter = filter;
    }
    
    public async Task<bool> IsUserExistsAsync(string userId)
    {
        // 检查用户ID是否可能存在
        return await _filter.ExistsAsync(userId);
    }
    
    public async Task AddNewUserAsync(string userId)
    {
        // 添加新用户ID到过滤器
        await _filter.AddAsync(userId);
    }
}
```

### 2. 批量操作

```csharp
public class ProductService
{
    private readonly IBloomFilter _filter;
    
    public async Task AddProductsAsync(List<string> productIds)
    {
        // 批量添加商品ID
        await _filter.AddAsync(productIds);
    }
    
    public async Task<List<string>> FilterNonExistingProductsAsync(
        List<string> productIds)
    {
        // 批量检查商品ID是否存在
        var exists = await _filter.ExistsAsync(productIds);
        
        return productIds
            .Where((id, index) => !exists[index])
            .ToList();
    }
}
```

## ⚙️ 最佳实践

### 1. 配置优化

```csharp
services.AddBloomFilter(options =>
{
    options.Name = "UserFilter";
    // 预期存储100万个元素
    options.Capacity = 1_000_000;
    // 期望的误判率为0.01
    options.ErrorRate = 0.01;
});
```

### 2. 缓存穿透防护

```csharp
public async Task<User> GetUserAsync(string userId)
{
    // 首先检查布隆过滤器
    if (!await _filter.ExistsAsync(userId))
        return null; // 用户一定不存在
        
    // 检查缓存
    var user = await _cache.GetAsync<User>(userId);
    if (user != null)
        return user;
        
    // 查询数据库
    user = await _repository.GetUserAsync(userId);
    if (user != null)
    {
        await _cache.SetAsync(userId, user);
        await _filter.AddAsync(userId);
    }
    
    return user;
}
```

## 🔨 项目依赖

- Microsoft.Extensions.DependencyInjection.Abstractions
- Microsoft.Extensions.Hosting.Abstractions
- System.Threading.Tasks

## 📝 使用注意事项

### 1. 容量规划
- 根据实际需求估算元素数量
- 合理设置误判率
- 预留足够的增长空间

### 2. 性能优化
- 使用批量操作减少网络请求
- 合理使用异步操作
- 注意内存使用

### 3. 应用场景
- 缓存穿透防护
- 重复数据检测
- 黑名单过滤

## 🤝 参与贡献

欢迎参与项目贡献！请阅读我们的[贡献指南](../CONTRIBUTING.md)了解如何参与项目开发。

## 📄 开源协议

本项目采用 MIT 开源协议 - 详情请查看 [LICENSE](../LICENSE) 文件。
