# Hangfire Sample

这是一个使用 `Tenon.Hangfire.Extensions` 的示例项目，展示了如何配置和使用 Hangfire 仪表板的认证和授权功能。

## 功能特点

- 基本认证（用户名/密码）
- IP 白名单认证
- 密码复杂度验证
- 登录失败次数限制
- SQLite 存储

## 配置说明

### 1. 缓存提供程序

本示例项目使用 `Tenon.Caching.InMemory` 作为缓存提供程序，这是一个基于内存的轻量级缓存实现。在 `Program.cs` 中配置如下：

```csharp
// 注册缓存提供程序
builder.Services.AddSingleton<ICacheProvider>(provider => 
    new MemoryCacheProvider(
        cacheName: "HangfireSample",        // 缓存实例名称
        cacheMemoryLimitMegabytes: 100,     // 限制使用内存为 100MB
        physicalMemoryLimitPercentage: 10,   // 限制使用物理内存的 10%
        pollingInterval: TimeSpan.FromMinutes(5) // 每 5 分钟清理过期缓存
    ));
```

缓存配置说明：
- `cacheName`: 缓存实例的唯一标识名称
- `cacheMemoryLimitMegabytes`: 缓存可使用的最大内存限制（MB）
- `physicalMemoryLimitPercentage`: 可使用的物理内存百分比
- `pollingInterval`: 自动清理过期缓存的时间间隔

如果您需要使用其他缓存提供程序，可以替换为：

- `Tenon.Caching.Redis`：基于 Redis 的缓存实现
- `Tenon.Caching.RedisStackExchange`：基于 StackExchange.Redis 的缓存实现

要使用其他缓存提供程序，只需替换 `ICacheProvider` 的实现即可：

```csharp
// 使用 Redis
services.AddSingleton<ICacheProvider, RedisCacheProvider>();

// 或者使用 StackExchange.Redis
services.AddSingleton<ICacheProvider, RedisStackExchangeCacheProvider>();
```

### 2. Hangfire 配置

在 `appsettings.json` 中配置 Hangfire 选项：

```json
{
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

## 运行项目

1. 确保已安装 .NET 9.0 SDK
2. 在项目根目录运行：
   ```bash
   dotnet run
   ```
3. 访问 Hangfire 仪表板：`https://localhost:5001/hangfire`

## 注意事项

- 在生产环境中，建议启用 `IgnoreAntiforgeryToken` 选项
- 请根据实际需求配置 IP 白名单
- 建议定期更改管理员密码
- 确保密码符合复杂度要求 