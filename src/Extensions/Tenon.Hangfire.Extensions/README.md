# Tenon.Hangfire.Extensions

Tenon.Hangfire.Extensions 是一个基于 Hangfire 的扩展库，提供了更多的功能和更好的配置选项，使 Hangfire 在 .NET 项目中的使用更加便捷和安全。

## 功能特性

- 增强的认证功能
  - 基本认证 (Basic Authentication)
  - 密码复杂度验证
  - 登录失败限制
  - IP 白名单控制

- 配置优化
  - 灵活的配置选项
  - 支持多种存储方式
  - 仪表板自定义

## 安装

```bash
dotnet add package Tenon.Hangfire.Extensions
```

## 快速开始

1. 在 `appsettings.json` 中添加配置：

```json
{
  "ConnectionStrings": {
    "HangfireConnection": "hangfire.db3"
  },
  "Hangfire": {
    "Path": "/hangfire",
    "DashboardTitle": "任务调度中心",
    "Authentication": {
      "Username": "admin",
      "Password": "Admin@123",
      "AuthType": "Basic",
      "EnablePasswordComplexity": true,
      "MinPasswordLength": 8,
      "RequireDigit": true,
      "RequireLowercase": true,
      "RequireUppercase": true,
      "RequireSpecialCharacter": true,
      "MaxLoginAttempts": 5,
      "LockoutDuration": 30
    },
    "IpAuthorization": {
      "Enabled": true,
      "AllowedIPs": [ "127.0.0.1", "::1" ],
      "AllowedIpRanges": [ "192.168.1.0/24" ]
    }
  }
}
```

2. 在 `Program.cs` 中配置服务：

```csharp
// 配置 Hangfire 选项
var hangfireSection = builder.Configuration.GetSection("Hangfire");
builder.Services.Configure<HangfireOptions>(hangfireSection);

// 配置认证选项
var authSection = hangfireSection.GetSection("Authentication");
builder.Services.Configure<AuthenticationOptions>(authSection);

// 添加 Hangfire 服务
builder.Services.AddHangfireServices(builder.Configuration);

// 配置 SQLite 存储选项
var storageOptions = new SQLiteStorageOptions
{
    // 基础配置
    Prefix = "hangfire",                            // 表前缀
    QueuePollInterval = TimeSpan.FromSeconds(15),   // 队列轮询间隔
    InvisibilityTimeout = TimeSpan.FromMinutes(30), // 任务隐藏超时
    DistributedLockLifetime = TimeSpan.FromSeconds(30), // 分布式锁超时
    
    // 维护配置
    JobExpirationCheckInterval = TimeSpan.FromHours(1),   // 过期任务检查间隔
    CountersAggregateInterval = TimeSpan.FromMinutes(5),  // 计数器聚合间隔
    
    // 性能配置
    PoolSize = 50,                                  // 连接池大小
    JournalMode = SQLiteStorageOptions.JournalModes.WAL,  // WAL模式提高并发性能
    AutoVacuumSelected = SQLiteStorageOptions.AutoVacuum.INCREMENTAL // 增量式自动清理
};

// 添加 Hangfire 服务
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSQLiteStorage(builder.Configuration.GetConnectionString("HangfireConnection"), storageOptions));

// 配置 Hangfire 服务器选项
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 2; // 工作线程数
    options.Queues = new[] { "default", "critical" }; // 任务队列
    options.ServerTimeout = TimeSpan.FromMinutes(5); // 服务器超时
    options.ShutdownTimeout = TimeSpan.FromMinutes(2); // 关闭超时
    options.ServerName = $"Hangfire.Server.{Environment.MachineName}"; // 服务器名称
});
```

3. 在中间件管道中启用 Hangfire：

```csharp
app.UseHangfire(app.Configuration.GetSection("Hangfire"));
```

## 配置说明

### 认证配置 (Authentication)

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| Username | 管理面板登录用户名 | admin |
| Password | 管理面板登录密码 | - |
| AuthType | 认证类型 (Basic) | Basic |
| EnablePasswordComplexity | 启用密码复杂度检查 | true |
| MinPasswordLength | 最小密码长度 | 8 |
| RequireDigit | 要求包含数字 | true |
| RequireLowercase | 要求包含小写字母 | true |
| RequireUppercase | 要求包含大写字母 | true |
| RequireSpecialCharacter | 要求包含特殊字符 | true |
| MaxLoginAttempts | 最大登录尝试次数 | 5 |
| LockoutDuration | 锁定时长（分钟） | 30 |

### IP 授权配置 (IpAuthorization)

| 配置项 | 说明 | 示例 |
|--------|------|------|
| Enabled | 启用 IP 白名单 | true |
| AllowedIPs | 允许的 IP 地址列表 | ["127.0.0.1", "::1"] |
| AllowedIpRanges | 允许的 IP 地址段 | ["192.168.1.0/24"] |

### 存储配置 (SQLiteStorageOptions)

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| Prefix | 数据库表前缀 | hangfire |
| QueuePollInterval | 队列轮询间隔 | 15秒 |
| InvisibilityTimeout | 任务隐藏超时 | 30分钟 |
| DistributedLockLifetime | 分布式锁超时 | 30秒 |
| JobExpirationCheckInterval | 过期任务检查间隔 | 1小时 |
| CountersAggregateInterval | 计数器聚合间隔 | 5分钟 |
| PoolSize | 连接池大小 | 50 |
| JournalMode | 日志模式 | WAL |
| AutoVacuumSelected | 自动清理模式 | INCREMENTAL |

## 依赖项

- .NET 9.0
- Hangfire (1.8.17)
- Microsoft.Extensions.Configuration (9.0.0)
- Microsoft.Extensions.DependencyInjection (9.0.0)
- Microsoft.Extensions.Options (9.0.0)

## 许可证

MIT

## 贡献

欢迎提交问题和建议到我们的 GitHub 仓库。 