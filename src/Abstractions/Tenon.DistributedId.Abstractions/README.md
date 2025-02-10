# Tenon.DistributedId.Abstractions

[![NuGet version](https://badge.fury.io/nu/Tenon.DistributedId.Abstractions.svg)](https://badge.fury.io/nu/Tenon.DistributedId.Abstractions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Tenon.DistributedId.Abstractions 提供了统一的分布式 ID 生成器抽象接口定义，是 Tenon 框架分布式 ID 生成功能的核心基础。通过抽象接口设计，实现了 ID 生成器的可插拔性和一致性。

## ✨ 设计优势

- 🎯 **统一抽象**：提供统一的 `IDGenerator` 接口，确保不同 ID 生成实现的一致性
- 🔌 **可插拔性**：支持多种 ID 生成算法无缝切换，无需修改业务代码
- 💡 **简洁接口**：精心设计的 API 接口，易于使用和扩展
- 🛡️ **类型安全**：严格的类型设计，避免运行时类型错误
- 🔄 **工作节点管理**：完善的工作节点管理机制，确保分布式环境下的唯一性
- 📦 **扩展机制**：灵活的选项扩展机制，支持自定义实现
- ⚡ **高性能**：优化的接口设计，最小化性能开销

## 📦 安装方式

通过 NuGet 包管理器安装：
```bash
dotnet add package Tenon.DistributedId.Abstractions
```

## 🚀 核心接口

### IDGenerator

提供统一的 ID 生成器接口：

```csharp
public interface IDGenerator
{
    // 设置工作节点 ID
    void SetWorkerId(ushort workerId);

    // 重置工作节点 ID
    void ResetWorkerId();

    // 获取下一个唯一 ID
    long GetNextId();

    // 当前工作节点 ID
    short? WorkerId { get; }

    // 最大工作节点 ID
    short MaxWorkerId { get; }
}
```

### IDistributedIdOptionsExtension

提供统一的选项扩展接口：

```csharp
public interface IDistributedIdOptionsExtension
{
    // 添加服务到依赖注入容器
    void AddServices(IServiceCollection services);
}
```

## 📖 使用方式

### 1. 注册服务

```csharp
services.AddDistributedId(options =>
{
    // 使用雪花算法实现
    options.UseSnowflake(snowflakeOptions => 
    {
        snowflakeOptions.ServiceName = "OrderService";
        snowflakeOptions.WorkerNode = new WorkerNodeOptions 
        {
            Prefix = "distributedId:workerIds:",
            ExpireTimeInSeconds = 60
        };
    });
});
```

### 2. 在服务中使用

```csharp
public class OrderService
{
    private readonly IDGenerator _idGenerator;

    public OrderService(IDGenerator idGenerator)
    {
        _idGenerator = idGenerator;
    }

    public long CreateOrderId()
    {
        return _idGenerator.GetNextId();
    }
}
```

## 💡 实现参考

框架提供了多个开箱即用的实现：

1. [Tenon.DistributedId.Snowflake](../../Tenon.DistributedId.Snowflake/README.md)
   - 基于雪花算法的分布式 ID 生成器实现
   - 支持 Redis 工作节点管理
   - 提供完整的配置选项

2. 自定义实现
   - 实现 `IDGenerator` 接口
   - 实现 `IDistributedIdOptionsExtension` 接口
   - 参考现有实现的最佳实践

## ⚙️ 配置选项

### DistributedIdOptions

分布式 ID 生成器的基础配置选项：

```csharp
public class DistributedIdOptions
{
    // 扩展集合
    public IList<IDistributedIdOptionsExtension> Extensions { get; }
    
    // 添加扩展
    public void RegisterExtension(IDistributedIdOptionsExtension extension);
}
```

## 🔨 项目依赖

- Microsoft.Extensions.DependencyInjection.Abstractions

## 📝 最佳实践

### 1. 接口实现
- 确保线程安全
- 实现完整的异常处理
- 添加适当的日志记录

### 2. 扩展开发
- 遵循依赖注入原则
- 实现优雅的配置机制
- 提供合理的默认值

### 3. 性能优化
- 避免不必要的对象分配
- 使用高效的算法
- 实现适当的缓存策略

## 🌰 实现示例

### 1. 基础实现

```csharp
public class CustomIdGenerator : IDGenerator
{
    private short? _workerId;
    
    public void SetWorkerId(ushort workerId)
    {
        _workerId = (short)workerId;
    }

    public void ResetWorkerId()
    {
        _workerId = null;
    }

    public long GetNextId()
    {
        if (!_workerId.HasValue)
            throw new InvalidOperationException("WorkerId not set");
            
        // 实现自定义的 ID 生成逻辑
        return GenerateId();
    }

    public short? WorkerId => _workerId;

    public short MaxWorkerId => 1024;
}
```

### 2. 扩展实现

```csharp
public class CustomOptionsExtension : IDistributedIdOptionsExtension
{
    private readonly CustomOptions _options;

    public CustomOptionsExtension(CustomOptions options)
    {
        _options = options;
    }

    public void AddServices(IServiceCollection services)
    {
        services.Configure<CustomOptions>(_options);
        services.AddSingleton<IDGenerator, CustomIdGenerator>();
    }
}
```

## 🤝 参与贡献

欢迎参与项目贡献！请阅读我们的[贡献指南](../../../CONTRIBUTING.md)了解如何参与项目开发。

## 📄 开源协议

本项目采用 MIT 开源协议 - 详情请查看 [LICENSE](../../../LICENSE) 文件。
