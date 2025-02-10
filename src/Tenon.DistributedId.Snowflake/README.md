# Tenon.DistributedId.Snowflake

[![NuGet version](https://badge.fury.io/nu/Tenon.DistributedId.Snowflake.svg)](https://badge.fury.io/nu/Tenon.DistributedId.Snowflake)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

基于 Yitter.IdGenerator 的分布式 ID 生成器实现，为 .NET 应用程序提供高性能、可靠的分布式唯一 ID 生成服务。

## ✨ 功能特性

- 🚀 基于 Yitter.IdGenerator 的高性能实现
- 🔧 支持 Redis 工作节点管理
- 💉 集成 .NET 依赖注入框架
- 🎯 自动工作节点注册和注销
- 🔄 支持工作节点自动刷新
- 📊 完整的日志监控支持
- 🛡️ 完善的异常处理机制

## 📦 安装方式

通过 NuGet 包管理器安装：
```bash
dotnet add package Tenon.DistributedId.Snowflake
```

## 🚀 快速入门

### 1. 配置 appsettings.json

```json
{
  "SnowflakeId": {
    "ServiceName": "OrderService",
    "WorkerNode": {
      "Prefix": "distributedId:workerIds:",
      "ExpireTimeInSeconds": 60,
      "RefreshTimeInSeconds": 30,
      "Redis": {
        "ConnectionString": "localhost:6379,defaultDatabase=0"
      }
    }
  }
}
```

### 2. 注册服务

```csharp
// 添加分布式 ID 生成服务
services.AddDistributedId(options =>
{
    // 使用 Snowflake 算法
    options.UseSnowflake(configuration.GetSection("DistributedId"));
    // 使用 StackExchange.Redis 作为工作节点提供者
    options.UseWorkerNode<StackExchangeProvider>(
        configuration.GetSection("DistributedId:WorkerNode"));
});

// 或者使用委托配置
services.AddDistributedId(options => 
{
    options.UseSnowflake(snowflakeOptions => 
    {
        snowflakeOptions.ServiceName = "OrderService";
        snowflakeOptions.WorkerNode = new WorkerNodeOptions 
        {
            Prefix = "distributedId:workerIds:",
            ExpireTimeInSeconds = 60,
            RefreshTimeInSeconds = 30,
            Redis = new RedisOptions 
            {
                ConnectionString = "localhost:6379"
            }
        };
    });
});
```

### 3. 使用 ID 生成器

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

## 📖 工作节点管理

### 工作节点配置

```csharp
public class WorkerNodeOptions
{
    // Redis 键前缀
    public string Prefix { get; set; } = "distributedId:workerIds:";
    
    // 工作节点过期时间（秒）
    public int ExpireTimeInSeconds { get; set; } = 60;
    
    // 工作节点刷新时间（秒）
    public int RefreshTimeInSeconds { get; set; }
    
    // Redis 配置选项
    public RedisOptions Redis { get; set; }
}
```

### 工作节点生命周期

```csharp
// 服务启动时自动注册工作节点
public override async Task StartAsync(CancellationToken cancellationToken)
{
    await _workerNode.RegisterAsync();
    await base.StartAsync(cancellationToken);
}

// 服务停止时自动注销工作节点
public override async Task StopAsync(CancellationToken cancellationToken)
{
    await _workerNode.UnRegisterAsync();
    await base.StopAsync(cancellationToken);
}
```

## ⚙️ 配置选项说明

### 基础配置

| 配置项 | 说明 | 默认值 |
|------|------|--------|
| ServiceName | 服务名称（必填） | - |
| WorkerNode.Prefix | Redis 键前缀 | distributedId:workerIds: |
| WorkerNode.ExpireTimeInSeconds | 工作节点过期时间 | 60 |
| WorkerNode.RefreshTimeInSeconds | 工作节点刷新时间 | 0 |

### ID 生成器配置

基于 Yitter.IdGenerator 的配置：
- WorkerIdBitLength: 6 位
- SeqBitLength: 6 位
- 最大支持的工作节点数：2^6 = 64 个

## 🔨 项目依赖

- Tenon.DistributedId.Abstractions
- Tenon.Infra.Redis
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.Options
- Yitter.IdGenerator

## 📝 使用注意事项

### 1. Redis 配置
- 确保 Redis 连接可用
- 合理设置过期时间
- 配置适当的刷新间隔

### 2. 工作节点管理
- 服务名称必须唯一
- 监控节点注册状态
- 关注节点过期情况

### 3. 性能优化
- 合理设置 WorkerIdBitLength
- 适当配置 SeqBitLength
- 避免频繁重启服务

## 🌰 应用场景示例

### 1. 订单 ID 生成

```csharp
public class OrderIdGenerator
{
    private readonly IDGenerator _idGenerator;
    
    public string GenerateOrderId()
    {
        return _idGenerator.GetNextId().ToString("D18");
    }
}
```

### 2. 分布式主键生成

```csharp
public class EntityIdGenerator
{
    private readonly IDGenerator _idGenerator;
    
    public void SetEntityId<T>(T entity) where T : IEntity
    {
        if (entity.Id <= 0)
        {
            entity.Id = _idGenerator.GetNextId();
        }
    }
}
```

## 🔍 异常处理

项目定义了两种主要异常类型：

1. `IDGeneratorException`
   - ID 生成器异常基类
   - 处理 ID 生成相关的异常

2. `IdGeneratorWorkerNodeException`
   - 工作节点异常
   - 处理节点注册、注销等操作异常

## 🤝 参与贡献

欢迎参与项目贡献！请阅读我们的[贡献指南](../CONTRIBUTING.md)了解如何参与项目开发。

## 📄 开源协议

本项目采用 MIT 开源协议 - 详情请查看 [LICENSE](../LICENSE) 文件。
