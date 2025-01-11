# Tenon.AspNetCore.OpenApi.Extensions

这是一个基于 Scalar UI 的 OpenAPI 文档扩展库，提供了美观、现代的 API 文档界面。

## 特性

- 基于 Scalar UI 的现代化界面
- 支持 OAuth2 认证
- 支持自定义主题
- 完整的中文注释
- 简单易用的配置选项

## 依赖项

- Scalar.AspNetCore (1.2.74)
- Microsoft.AspNetCore.Authentication.JwtBearer (9.0.0)

## 安装

```bash
dotnet add package Tenon.AspNetCore.OpenApi.Extensions
```

## 使用方法

1. 在 `Program.cs` 中注册服务：

```csharp
// 添加 OpenAPI 服务
builder.Services.AddTenonOpenApi(options =>
{
    options.Title = "我的 API";
    options.Version = "v1";
    options.Description = "API 描述";
    
    // 配置 OAuth2（可选）
    options.OAuth2 = new OAuth2Options
    {
        Authority = "https://auth-server",
        ClientId = "client_id",
        Scopes = new List<string> { "api1" }
    };
    
    // 配置主题（可选）
    options.Theme = new ScalarThemeOptions
    {
        DarkMode = true,
        Colors = new Dictionary<string, string>
        {
            { "primary", "#1976d2" }
        }
    };
});

// 配置中间件（注意：必须在 UseRouting 之后）
app.UseRouting();
app.UseTenonOpenApi();
```

## 配置选项

### ScalarOptions

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Title | string | "API Documentation" | API 文档标题 |
| Version | string | "v1" | API 版本 |
| Description | string | "" | API 描述 |
| RoutePrefix | string | "api-docs" | 路由前缀 |
| EnableAuthorization | bool | true | 是否启用授权 |
| OAuth2 | OAuth2Options | null | OAuth2 配置 |
| Theme | ScalarThemeOptions | new() | 主题配置 |

### OAuth2Options

| 属性 | 类型 | 说明 |
|------|------|------|
| Authority | string | 授权服务器地址 |
| ClientId | string | 客户端ID |
| Scopes | List<string> | 授权范围 |

### ScalarThemeOptions

| 属性 | 类型 | 说明 |
|------|------|------|
| DarkMode | bool | 是否启用暗色主题 |
| Colors | Dictionary<string, string> | 主题颜色配置 |

## 注意事项

1. 必须在 `UseRouting` 之后调用 `UseTenonOpenApi`
2. API 文档默认访问路径为 `/api-docs`
3. OpenAPI JSON 文档路径为 `/api-docs/{documentName}.json`

## 许可证

MIT 