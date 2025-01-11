# Tenon.AspNetCore.OpenApi.Extensions

Scalar UI OpenAPI 扩展包，提供了更美观的 API 文档界面和更灵活的配置选项。

## 功能特性

- 集成 Scalar UI 作为 OpenAPI 文档界面
- 支持 JWT Bearer 认证
- 支持 OAuth2 认证配置
- 支持数组参数的逗号分隔格式
- 提供完整的 XML 文档注释
- 环境感知配置
- 主题定制支持

## 示例项目

完整的使用示例请参考 [OpenApiSample](../../samples/OpenApiSample/README.md) 项目，其中包含：
- JWT Bearer 认证示例
- OAuth2 配置示例
- 数组参数处理示例
- 主题定制示例

## 快速开始

1. 安装 NuGet 包：
```bash
dotnet add package Tenon.AspNetCore.OpenApi.Extensions
```

2. 注册服务：
```csharp
// 添加 OpenAPI 服务
builder.Services.AddScalarOpenApi(builder.Configuration.GetSection("ScalarUI"));

// 使用 OpenAPI 中间件（仅在开发环境）
if (app.Environment.IsDevelopment())
{
    app.UseScalarOpenApi();
}
```

## 配置说明

### appsettings.json 配置示例

```json
{
  "ScalarUI": {
    "Title": "API 文档",
    "Version": "v1",
    "Description": "API 接口文档",
    "OAuth2": {
      "Authority": "https://auth-server",
      "ClientId": "api_client",
      "Scopes": [ "api1" ]
    },
    "Theme": {
      "DarkMode": true,
      "Colors": {
        "primary": "#1976d2",
        "secondary": "#424242",
        "success": "#2e7d32",
        "error": "#d32f2f",
        "warning": "#ed6c02",
        "info": "#0288d1"
      }
    }
  }
}
```

### 配置方式

1. 环境感知配置：
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseScalarOpenApi();
}
```

2. 配置文件配置：
```csharp
builder.Services.AddScalarOpenApi(builder.Configuration.GetSection("ScalarUI"));
```

3. 代码配置：
```csharp
builder.Services.AddScalarOpenApi(options =>
{
    options.Title = "My API";
    options.Version = "v1";
    options.Theme.DarkMode = true;
});
```

## 高级功能

### 数组参数支持

1. URL 查询参数：
```csharp
[HttpGet]
public IEnumerable<string> Get(
    [FromQuery][ModelBinder(typeof(CommaDelimitedArrayModelBinder))] 
    int[] ids)
{
    // 支持 ?ids=1,2,3 格式
}
```

2. 请求体数组：
```csharp
public class BatchRequest
{
    [Required]
    public int[] Ids { get; set; } = Array.Empty<int>();
}

[HttpPost("batch")]
public IActionResult Post([FromBody] BatchRequest request)
{
    // 支持标准 JSON 数组格式
}
```

### 认证配置

1. JWT Bearer 认证：
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
```

2. OAuth2 认证：
```json
{
  "OAuth2": {
    "Authority": "https://auth-server",
    "ClientId": "api_client",
    "Scopes": [ "api1" ]
  }
}
```

## 最佳实践

1. 环境配置：
   - 开发环境启用 OpenAPI 文档
   - 生产环境禁用 OpenAPI 文档
   - 不同环境使用不同的认证配置

2. 安全配置：
   - 使用 HTTPS
   - 配置适当的认证方式
   - 启用 CORS 策略

3. 文档优化：
   - 添加完整的 XML 注释
   - 提供清晰的示例
   - 使用适当的响应类型

## 许可证

MIT 