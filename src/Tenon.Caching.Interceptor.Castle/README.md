# Tenon.Caching.Interceptor.Castle

[![NuGet version](https://badge.fury.io/nu/Tenon.Caching.Interceptor.Castle.svg)](https://badge.fury.io/nu/Tenon.Caching.Interceptor.Castle)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

基于 Castle DynamicProxy 的缓存拦截器实现，提供 Cache-Aside 模式和延时双删策略的 AOP 缓存解决方案。

## ✨ 功能特性

- 🚀 基于 Castle DynamicProxy 的轻量级实现
- 🔧 支持同步和异步方法拦截
- 💉 Cache-Aside 缓存模式
- 🎯 延时双删策略
- 🔄 失败补偿机制
- 📊 灵活的缓存键生成器
- 🛡️ 完整的异常处理

## 📦 安装方式

通过 NuGet 包管理器安装：
```bash
dotnet add package Tenon.Caching.Interceptor.Castle
```

## 🚀 快速入门

### 1. 注册服务

```csharp
services.AddCachingInterceptor(options =>
{
    // 配置延时双删时间间隔（毫秒）
    options.DelayDeleteMilliseconds = 500;
    
    // 配置失败重试次数
    options.RetryCount = 3;
    
    // 配置重试间隔（毫秒）
    options.RetryIntervalMilliseconds = 200;
});
```

### 2. 使用缓存特性

```csharp
public interface IProductService
{
    Task<Product> GetProductAsync(int id);
    Task UpdateProductAsync(Product product);
}

public class ProductService : IProductService
{
    [CachingAbl(ExpirationInSec = 3600)] // 缓存1小时
    public async Task<Product> GetProductAsync(int id)
    {
        // 从数据库获取商品
        return await _repository.GetByIdAsync(id);
    }

    [CachingEvict] // 更新时清除缓存
    public async Task UpdateProductAsync(Product product)
    {
        await _repository.UpdateAsync(product);
    }
}
```

### 3. 配置缓存参数

```csharp
public class OrderService
{
    [CachingAbl(ExpirationInSec = 1800)] // 缓存30分钟
    public async Task<Order> GetOrderAsync(
        [CachingParameter(Name = "orderId")] string id,
        [CachingParameter(Ignore = true)] string userId)
    {
        return await _repository.GetOrderAsync(id);
    }
}
```

## 📖 高级用法

### 自定义缓存键生成器

```csharp
public class CustomCacheKeyGenerator : ICacheKeyGenerator
{
    public string Generate(InvocationMetadata metadata)
    {
        var methodInfo = metadata.Method;
        var parameters = metadata.Parameters;
        
        // 自定义缓存键生成逻辑
        var key = $"{methodInfo.DeclaringType?.Name}:{methodInfo.Name}";
        
        foreach (var param in parameters)
        {
            if (!param.Ignore)
            {
                key += $":{param.Name}={param.Value}";
            }
        }
        
        return key;
    }
}

// 注册自定义生成器
services.AddSingleton<ICacheKeyGenerator, CustomCacheKeyGenerator>();
```

### 处理缓存异常

```csharp
public class ProductService
{
    [CachingAbl(ExpirationInSec = 3600)]
    public async Task<Product> GetProductWithRetryAsync(int id)
    {
        try
        {
            return await _repository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取商品信息失败");
            throw new CachingAblException(
                "获取商品信息失败", 
                ex);
        }
    }
}
```

## ⚙️ 特性说明

### CachingAbl 特性

用于标记需要缓存的方法：

```csharp
[CachingAbl(ExpirationInSec = 3600)] // 缓存1小时
public async Task<T> GetDataAsync<T>(string key)
{
    return await _repository.GetAsync<T>(key);
}
```

### CachingEvict 特性

用于标记需要清除缓存的方法：

```csharp
[CachingEvict]
public async Task UpdateDataAsync<T>(string key, T value)
{
    await _repository.UpdateAsync(key, value);
}
```

### CachingParameter 特性

用于自定义缓存键参数：

```csharp
public async Task<User> GetUserAsync(
    [CachingParameter(Name = "uid")] int userId,
    [CachingParameter(Ignore = true)] string trace)
{
    return await _repository.GetUserAsync(userId);
}
```

## 🔨 项目依赖

- Castle.Core
- Tenon.Caching.Abstractions
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Logging

## 📝 使用注意事项

### 1. 延时双删策略
- 合理配置延时时间
- 考虑数据一致性要求
- 注意性能影响

### 2. 异常处理
- 实现完整的异常处理机制
- 配置合适的重试策略
- 记录必要的错误日志

### 3. 最佳实践
- 合理设置缓存过期时间
- 避免缓存大对象
- 正确处理并发情况

## ✅ 示例场景

### 1. 商品缓存

```csharp
public class ProductService
{
    [CachingAbl(ExpirationInSec = 1800)]
    public async Task<List<Product>> GetHotProductsAsync(
        [CachingParameter] int categoryId,
        [CachingParameter] int limit)
    {
        return await _repository.GetHotProductsAsync(
            categoryId, 
            limit);
    }

    [CachingEvict]
    public async Task UpdateProductStockAsync(
        [CachingParameter] int productId,
        [CachingParameter] int stock)
    {
        await _repository.UpdateStockAsync(
            productId, 
            stock);
    }
}
```

### 2. 用户信息缓存

```csharp
public class UserService
{
    [CachingAbl(ExpirationInSec = 3600)]
    public async Task<UserInfo> GetUserInfoAsync(
        [CachingParameter] int userId)
    {
        return await _repository.GetUserInfoAsync(userId);
    }

    [CachingEvict]
    public async Task UpdateUserProfileAsync(
        [CachingParameter] UserProfile profile)
    {
        await _repository.UpdateProfileAsync(profile);
    }
}
```

## 🤝 参与贡献

欢迎参与项目贡献！请阅读我们的[贡献指南](../CONTRIBUTING.md)了解如何参与项目开发。

## 📄 开源协议

本项目采用 MIT 开源协议 - 详情请查看 [LICENSE](../LICENSE) 文件。
