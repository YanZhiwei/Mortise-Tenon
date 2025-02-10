# Tenon.BloomFilter.Redis

[![NuGet version](https://badge.fury.io/nu/Tenon.BloomFilter.Redis.svg)](https://badge.fury.io/nu/Tenon.BloomFilter.Redis)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Tenon.BloomFilter.Redis 是一个 Redis 布隆过滤器抽象层实现，为 Tenon 框架提供统一的 Redis 布隆过滤器接口和基础实现。

## ✨ 功能特性

- 🚀 轻量级 Redis 布隆过滤器抽象实现
- 🔧 统一的过滤器接口定义
- 💉 支持多种 Redis 客户端实现
- 🎯 完整的过滤器操作支持
- 🔄 灵活的配置选项
- 📊 可扩展的过滤器提供者
- 🛡️ 内置异常处理机制

## 📦 安装方式

通过 NuGet 包管理器安装：
```bash
dotnet add package Tenon.BloomFilter.Redis
```

## 🚀 快速入门

### 1. 实现布隆过滤器提供者

```csharp
public class CustomRedisBloomFilter : RedisBloomFilter
{
    public CustomRedisBloomFilter(
        IRedisProvider redisProvider,
        BloomFilterOptions options) 
        : base(redisProvider, options)
    {
    }

    // 可以在这里扩展或重写基类方法
    public override Task<bool> AddAsync(string value)
    {
        // 自定义实现
        return base.AddAsync(value);
    }
}
```

### 2. 注册服务

```csharp
services.AddSingleton<IBloomFilter>(sp => 
{
    var redisProvider = sp.GetRequiredService<IRedisProvider>();
    var options = new BloomFilterOptions
    {
        Name = "CustomFilter",
        Capacity = 1_000_000,
        ErrorRate = 0.01
    };
    
    return new CustomRedisBloomFilter(
        redisProvider, 
        options);
});
```

### 3. 使用布隆过滤器

```csharp
public class FilterService
{
    private readonly IBloomFilter _filter;

    public FilterService(IBloomFilter filter)
    {
        _filter = filter;
    }

    public async Task<bool> IsValueUniqueAsync(string value)
    {
        if (await _filter.ExistsAsync(value))
            return false;  // 值可能已存在

        await _filter.AddAsync(value);
        return true;  // 值之前一定不存在
    }
}
```

## 📖 高级用法

### 自定义过滤器实现

```csharp
public class ClusterRedisBloomFilter : RedisBloomFilter
{
    public ClusterRedisBloomFilter(
        IRedisProvider redisProvider,
        BloomFilterOptions options)
        : base(redisProvider, options)
    {
    }

    protected override async Task<bool> InitializeFilterAsync()
    {
        // 实现集群环境下的初始化逻辑
        return await base.InitializeFilterAsync();
    }

    protected override string GenerateKey(string value)
    {
        // 自定义键生成策略
        return $"{Options.Name}:{value}";
    }
}
```

### 过滤器管理

```csharp
public class BloomFilterManager
{
    private readonly Dictionary<string, IBloomFilter> _filters;
    
    public async Task<IBloomFilter> GetOrCreateFilterAsync(
        string name,
        int capacity,
        double errorRate)
    {
        if (_filters.TryGetValue(name, out var filter))
            return filter;
            
        var options = new BloomFilterOptions
        {
            Name = name,
            Capacity = capacity,
            ErrorRate = errorRate
        };
        
        filter = new RedisBloomFilter(_redisProvider, options);
        await filter.InitAsync();
        
        _filters[name] = filter;
        return filter;
    }
}
```

## ⚙️ 接口说明

### RedisBloomFilter

基础 Redis 布隆过滤器实现，包含：

- 基础过滤器操作实现
- Redis 键管理
- 异常处理和重试机制
- 过滤器初始化逻辑
- 批量操作支持

## 🔨 项目依赖

- Tenon.BloomFilter.Abstractions
- Tenon.Infra.Redis
- Microsoft.Extensions.DependencyInjection.Abstractions

## 📝 使用注意事项

### 1. 性能考虑
- 选择合适的哈希函数
- 优化 Redis 访问模式
- 注意批量操作性能

### 2. 容量规划
- 预估数据增长
- 合理设置误判率
- 监控内存使用

### 3. 最佳实践
- 实现监控和统计
- 定期维护过滤器
- 做好容灾备份

## 🌰 实际应用示例

### 1. 黑名单过滤

```csharp
public class BlacklistService
{
    private readonly IBloomFilter _filter;

    public async Task<bool> IsBlacklistedAsync(string ip)
    {
        return await _filter.ExistsAsync(ip);
    }

    public async Task AddToBlacklistAsync(string ip)
    {
        await _filter.AddAsync(ip);
        await _logger.LogAsync($"IP {ip} 已加入黑名单");
    }
}
```

### 2. 重复数据检测

```csharp
public class DuplicateChecker
{
    private readonly IBloomFilter _filter;

    public async Task<bool> IsDataProcessedAsync(string dataId)
    {
        if (await _filter.ExistsAsync(dataId))
            return true;  // 数据可能已处理

        await _filter.AddAsync(dataId);
        return false;  // 数据一定未处理
    }
}
```

## 🔗 相关实现

- [Tenon.BloomFilter.Abstractions](../Abstractions/Tenon.BloomFilter.Abstractions/README.md) - 布隆过滤器抽象接口
- [Tenon.BloomFilter.RedisStackExchange](../Tenon.BloomFilter.RedisStackExchange/README.md) - StackExchange.Redis 实现

## 🤝 参与贡献

欢迎参与项目贡献！请阅读我们的[贡献指南](../CONTRIBUTING.md)了解如何参与项目开发。

## 📄 开源协议

本项目采用 MIT 开源协议 - 详情请查看 [LICENSE](../LICENSE) 文件。
