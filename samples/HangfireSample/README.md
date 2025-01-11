# HangfireSample

这是一个使用 Hangfire 的示例项目，展示了如何在 ASP.NET Core 项目中集成和使用 Hangfire 进行后台任务处理。

## 功能特性

- 集成 Hangfire 用于后台任务处理
- 使用 SQLite 作为 Hangfire 的存储
- 实现自定义的内存缓存提供程序
- 提供基本的认证和 IP 授权
- 包含示例任务服务

## 项目结构

```
HangfireSample/
├── Caching/                    # 缓存相关实现
│   └── HangfireMemoryCacheProvider.cs  # Hangfire 专用内存缓存提供程序
├── Services/                   # 服务实现
│   └── SampleJobService.cs     # 示例任务服务
├── Controllers/               # API 控制器
├── Program.cs                 # 应用程序入口点
└── appsettings.json          # 应用程序配置文件
```

## 配置说明

在 `appsettings.json` 中配置 Hangfire：

```json
{
  "Hangfire": {
    "Path": "/hangfire",
    "DashboardTitle": "任务管理面板",
    "Authentication": {
      "Username": "admin",
      "Password": "123456",
      "EnablePasswordComplexity": false
    },
    "IpAuthorization": {
      "Enabled": true,
      "AllowedIps": ["127.0.0.1", "::1"]
    }
  },
  "ConnectionStrings": {
    "HangfireConnection": "Data Source=hangfire.db;"
  }
}
```

## 使用方法

1. 注册服务：

```csharp
// 注册 Hangfire 缓存提供程序
builder.Services.AddSingleton<IHangfireCacheProvider, HangfireMemoryCacheProvider>();

// 添加 Hangfire 服务
builder.Services.AddHangfireServices(
    builder.Configuration,
    configureStorage: config =>
    {
        // 配置 SQLite 存储
        var storageOptions = new SQLiteStorageOptions
        {
            Prefix = "hangfire",
            QueuePollInterval = TimeSpan.FromSeconds(15),
            InvisibilityTimeout = TimeSpan.FromMinutes(30),
            DistributedLockLifetime = TimeSpan.FromSeconds(30)
        };

        config.UseSQLiteStorage(
            builder.Configuration.GetConnectionString("HangfireConnection"),
            storageOptions);
    });
```

2. 配置中间件：

```csharp
// 使用 Hangfire
app.UseHangfire(app.Configuration.GetSection("Hangfire"));
```

3. 创建后台任务：

```csharp
public class SampleJobService
{
    public void DoSampleJob()
    {
        // 实现你的任务逻辑
    }
}

// 注册任务
BackgroundJob.Enqueue<SampleJobService>(x => x.DoSampleJob());
```

## 注意事项

1. 确保正确配置 SQLite 数据库连接字符串
2. 在生产环境中修改默认的用户名和密码
3. 根据实际需求配置 IP 白名单
4. 在生产环境中建议启用 `IgnoreAntiforgeryToken`

## 依赖项

- Hangfire.AspNetCore
- Hangfire.Storage.SQLite
- System.Data.SQLite.Core
- Tenon.Hangfire.Extensions 