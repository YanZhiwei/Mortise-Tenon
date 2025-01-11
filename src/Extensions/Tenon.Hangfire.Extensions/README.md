# Tenon.Hangfire.Extensions

Tenon.Hangfire.Extensions 是一个用于增强 Hangfire 功能的扩展库，提供了认证、授权和缓存等功能的统一实现。

## 功能特性

- 基本认证（用户名/密码）
- IP 白名单认证
- 密码复杂度验证
- 登录失败次数限制
- 可扩展的缓存提供程序

## 安装

```bash
dotnet add package Tenon.Hangfire.Extensions
```

## 使用方法

### 1. 基本配置

在 `appsettings.json` 中添加配置：

```json
{
  "Hangfire": {
    "Path": "/hangfire",
    "DashboardTitle": "任务管理面板",
    "Authentication": {
      "Username": "admin",
      "Password": "Admin@123",
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
      "AllowedIps": ["127.0.0.1", "::1"]
    }
  }
}
```

### 2. 注册服务

```csharp
// 注册缓存提供程序
services.AddSingleton<IHangfireCacheProvider, YourCacheProvider>();

// 添加 Hangfire 服务
services.AddHangfireServices(
    configuration,
    configureStorage: config =>
    {
        // 配置你的存储
    });
```

### 3. 配置中间件

```csharp
app.UseHangfire(app.Configuration.GetSection("Hangfire"));
```

## 配置说明

### HangfireOptions

| 属性 | 说明 | 默认值 |
|------|------|--------|
| Path | 仪表板路径 | "/hangfire" |
| DashboardTitle | 仪表板标题 | "Hangfire" |

### AuthenticationOptions

| 属性 | 说明 | 默认值 |
|------|------|--------|
| Username | 用户名 | - |
| Password | 密码 | - |
| EnablePasswordComplexity | 启用密码复杂度验证 | false |
| MinPasswordLength | 最小密码长度 | 8 |
| RequireDigit | 要求包含数字 | false |
| RequireLowercase | 要求包含小写字母 | false |
| RequireUppercase | 要求包含大写字母 | false |
| RequireSpecialCharacter | 要求包含特殊字符 | false |
| MaxLoginAttempts | 最大登录尝试次数 | 5 |
| LockoutDuration | 锁定时长（分钟） | 30 |

### IpAuthorizationOptions

| 属性 | 说明 | 默认值 |
|------|------|--------|
| Enabled | 启用 IP 授权 | false |
| AllowedIps | 允许的 IP 列表 | [] |

## 自定义缓存提供程序

实现 `IHangfireCacheProvider` 接口来创建自定义缓存提供程序：

```csharp
public class CustomCacheProvider : IHangfireCacheProvider
{
    public bool Set<T>(string cacheKey, T cacheValue, TimeSpan expiration)
    {
        // 实现缓存设置逻辑
    }

    public CacheValue<T> Get<T>(string cacheKey)
    {
        // 实现缓存获取逻辑
    }

    // 实现其他接口方法...
}
```

## 最佳实践

1. **安全性**
   - 在生产环境中使用强密码
   - 配置适当的 IP 白名单
   - 启用密码复杂度验证

2. **性能**
   - 选择合适的缓存提供程序
   - 合理配置缓存过期时间
   - 监控登录失败次数

3. **可维护性**
   - 使用配置文件管理设置
   - 实现自定义的日志记录
   - 定期更新密码和白名单

## 依赖项

- Hangfire.AspNetCore
- Hangfire.Core
- Microsoft.AspNetCore.Http.Abstractions
- Microsoft.Extensions.Configuration.Abstractions
- Microsoft.Extensions.DependencyInjection.Abstractions
- Microsoft.Extensions.Logging.Abstractions
- Microsoft.Extensions.Options.ConfigurationExtensions 