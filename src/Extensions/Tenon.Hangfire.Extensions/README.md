# Tenon.Hangfire.Extensions

Hangfire 扩展包，提供了更灵活的配置选项和额外的功能特性。

## 功能特性

- IP 白名单授权
- 基本认证支持
- 灵活的配置选项
- 内置缓存支持
- 环境感知配置
- 日志记录增强

## 示例项目

完整的使用示例请参考 [HangfireSample](../../samples/HangfireSample/README.md) 项目，其中包含：
- SQLite 存储配置示例
- 多种服务器配置方式
- 完整的安全认证配置
- 各类任务示例代码
- 性能优化建议

## 快速开始

1. 安装 NuGet 包：
```bash
dotnet add package Tenon.Hangfire.Extensions
```

2. 注册服务：
```csharp
// 注册 Hangfire 缓存提供程序
services.AddSingleton<IHangfireCacheProvider, HangfireMemoryCacheProvider>();

// 添加 Hangfire 服务
services.AddHangfireServices(
    configuration.GetSection("Hangfire"),
    configureStorage: config => 
    {
        // 配置存储
    },
    configureServer: options => 
    {
        // 配置服务器选项
    });
```

3. 配置中间件：
```csharp
app.UseHangfire();
```

## 配置说明

### appsettings.json 配置示例

```json
{
  "Hangfire": {
    "Path": "/hangfire",
    "DashboardTitle": "任务调度中心",
    "IgnoreAntiforgeryToken": true,
    "SkipBasicAuthenticationIfIpAuthorized": true,
    
    "Server": {
      "WorkerCount": 10,
      "Queues": [ "critical", "default", "low" ],
      "ServerTimeoutMinutes": 5,
      "ShutdownTimeoutMinutes": 2,
      "ServerName": "CustomHangfireServer"
    },
    
    "IpAuthorization": {
      "Enabled": true,
      "AllowedIPs": [ "127.0.0.1", "::1" ],
      "AllowedIpRanges": [ "192.168.1.0/24" ]
    },
    
    "Authentication": {
      "Username": "admin",
      "Password": "123456",
      "MaxFailedAttempts": 3,
      "LockoutTime": "00:05:00"
    }
  }
}
```

### 服务器配置方式

1. 环境感知配置：
```csharp
if (environment.IsDevelopment())
{
    options.WorkerCount = 5;
    options.Queues = new[] { "development", "default" };
}
```

2. 配置文件配置：
```csharp
var serverSection = configuration.GetSection("Hangfire:Server");
if (serverSection.Exists())
{
    var workerCount = serverSection.GetValue<int?>("WorkerCount");
    if (workerCount.HasValue)
    {
        options.WorkerCount = workerCount.Value;
    }
}
```

3. 环境变量配置：
```csharp
var envWorkerCount = Environment.GetEnvironmentVariable("HANGFIRE_WORKER_COUNT");
if (!string.IsNullOrEmpty(envWorkerCount) && int.TryParse(envWorkerCount, out var count))
{
    options.WorkerCount = count;
}
```

4. 动态计算配置：
```csharp
options.WorkerCount = Math.Min(
    Environment.ProcessorCount * 5,
    Math.Max(5, Environment.ProcessorCount * 2)
);
```

## 安全特性

### IP 授权
- 支持具体 IP 地址白名单
- 支持 CIDR 格式的 IP 范围
- 可选择性跳过基本认证

### 基本认证
- 用户名密码认证
- 登录失败次数限制
- 账户锁定机制
- 密码复杂度验证

## 日志记录

内置详细的日志记录：
```csharp
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Hangfire": "Information",
      "Tenon.Hangfire.Extensions.Filters": "Debug"
    }
  }
}
```

## 最佳实践

1. 环境配置：
   - 开发环境使用较少的工作线程
   - 生产环境根据负载调整线程数
   - 不同环境使用不同的队列优先级

2. 安全配置：
   - 生产环境禁用 IgnoreAntiforgeryToken
   - 使用复杂的密码
   - 配置适当的 IP 白名单

3. 性能优化：
   - 根据 CPU 核心数配置工作线程
   - 合理设置队列优先级
   - 适当配置超时时间

## 许可证

MIT 