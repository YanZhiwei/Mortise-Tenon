# HangfireSample

Tenon.Hangfire.Extensions 的示例项目，展示了如何使用和配置 Hangfire 扩展包。

## 项目特点

- 使用 SQLite 作为存储
- 完整的配置示例
- 多种服务器配置方式
- 安全认证示例

## 快速开始

1. 克隆仓库：
```bash
git clone <repository-url>
cd samples/HangfireSample
```

2. 运行项目：
```bash
dotnet run
```

3. 访问仪表板：
```
http://localhost:5000/hangfire
```

## 配置说明

### 存储配置

使用 SQLite 作为 Hangfire 的存储：
```csharp
configureStorage: config =>
{
    var storageOptions = new SQLiteStorageOptions
    {
        // 基础配置
        Prefix = "hangfire",
        QueuePollInterval = TimeSpan.FromSeconds(15),
        InvisibilityTimeout = TimeSpan.FromMinutes(30),
        DistributedLockLifetime = TimeSpan.FromSeconds(30),

        // 维护配置
        JobExpirationCheckInterval = TimeSpan.FromHours(1),
        CountersAggregateInterval = TimeSpan.FromMinutes(5),

        // 性能配置
        PoolSize = Environment.ProcessorCount * 2,
        JournalMode = SQLiteStorageOptions.JournalModes.WAL,
        AutoVacuumSelected = SQLiteStorageOptions.AutoVacuum.INCREMENTAL
    };

    config.UseSQLiteStorage(
        "hangfire.db3",
        storageOptions);
}
```

### 服务器配置

示例展示了四种不同的配置方式：

1. 环境感知配置：
```csharp
if (builder.Environment.IsDevelopment())
{
    options.WorkerCount = 5;
    options.Queues = new[] { "development", "default" };
    options.ServerTimeout = TimeSpan.FromMinutes(2);
    options.ServerName = $"Dev.{Environment.MachineName}";
}
```

2. 配置文件配置：
```csharp
var serverSection = builder.Configuration.GetSection("Hangfire:Server");
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

### 安全配置

在 `appsettings.json` 中配置认证和授权：

```json
{
  "Hangfire": {
    "SkipBasicAuthenticationIfIpAuthorized": true,
    "IpAuthorization": {
      "Enabled": true,
      "AllowedIPs": [ "127.0.0.1", "::1" ]
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

## 示例任务

项目包含了一些示例任务的实现：

1. 简单任务：
```csharp
BackgroundJob.Enqueue(() => Console.WriteLine("Simple Task"));
```

2. 延迟任务：
```csharp
BackgroundJob.Schedule(
    () => Console.WriteLine("Delayed Task"),
    TimeSpan.FromMinutes(5));
```

3. 循环任务：
```csharp
RecurringJob.AddOrUpdate(
    "daily-cleanup",
    () => Console.WriteLine("Daily Cleanup"),
    Cron.Daily);
```

## 日志配置

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Hangfire": "Information",
      "Tenon.Hangfire.Extensions.Filters": "Debug"
    }
  }
}
```

## 项目结构

```
HangfireSample/
├── Controllers/        # API 控制器
├── Services/          # 示例任务服务
├── Caching/          # 缓存实现
├── appsettings.json  # 配置文件
└── Program.cs        # 应用程序入口
```

## 依赖项

- Hangfire.Storage.SQLite
- Tenon.Hangfire.Extensions
- Tenon.Caching.InMemory
- Microsoft.AspNetCore.OpenApi
- Scalar.AspNetCore

## 注意事项

1. 开发环境：
   - 使用较少的工作线程
   - 启用详细日志
   - 禁用防伪令牌验证

2. 生产环境：
   - 使用适当的工作线程数
   - 配置合适的队列优先级
   - 启用所有安全特性

3. 性能优化：
   - 合理配置 SQLite 存储选项
   - 根据负载调整工作线程数
   - 设置合适的任务超时时间

## 许可证

MIT 