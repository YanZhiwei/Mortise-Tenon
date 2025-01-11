# OpenApiSample

Tenon.AspNetCore.OpenApi.Extensions 的示例项目，展示了如何使用和配置 Scalar UI OpenAPI 扩展包。

## 项目特点

- JWT Bearer 认证集成
- OAuth2 认证示例
- 数组参数处理
- 主题定制
- 完整的 XML 文档

## 快速开始

1. 克隆仓库：
```bash
git clone <repository-url>
cd samples/OpenApiSample
```

2. 运行项目：
```bash
dotnet run
```

3. 访问 API 文档：
```
http://localhost:5000/scalar/v1
```

## 配置说明

### 认证配置

使用 JWT Bearer 和 OAuth2 认证：
```json
{
  "Jwt": {
    "Authority": "https://localhost:5001",
    "ClientId": "weather_api_client",
    "Audience": "weather_api"
  },
  "ScalarUI": {
    "OAuth2": {
      "Authority": "https://localhost:5001",
      "ClientId": "weather_api_client",
      "Scopes": [ "weather_api" ]
    }
  }
}
```

### 主题配置

自定义 Scalar UI 主题：
```json
{
  "ScalarUI": {
    "Theme": {
      "DarkMode": true,
      "Colors": {
        "primary": "#1976d2",
        "secondary": "#424242",
        "success": "#2e7d32",
        "error": "#d32f2f"
      }
    }
  }
}
```

## API 示例

### 1. 获取天气预报（GET）

支持逗号分隔的数组参数：
```http
GET /WeatherForecast?days=1,2,3
Authorization: Bearer <token>
```

响应示例：
```json
[
  {
    "date": "2024-01-21",
    "temperatureC": 20,
    "summary": "Warm"
  }
]
```

### 2. 批量获取天气预报（POST）

支持请求体中的数组：
```http
POST /WeatherForecast/batch
Content-Type: application/json
Authorization: Bearer <token>

{
  "cities": ["北京", "上海", "广州"],
  "days": [1, 2, 3]
}
```

响应示例：
```json
[
  {
    "date": "2024-01-21",
    "temperatureC": 20,
    "summary": "北京: Warm"
  }
]
```

## 项目结构

```
OpenApiSample/
├── Controllers/
│   └── WeatherForecastController.cs    # API 控制器
├── Models/
│   ├── WeatherForecast.cs              # 天气预报模型
│   └── WeatherForecastRequest.cs       # 请求模型
├── Program.cs                          # 应用程序入口
├── appsettings.json                    # 主配置文件
└── appsettings.Development.json        # 开发环境配置
```

## 依赖项

- Tenon.AspNetCore.OpenApi.Extensions
- Microsoft.AspNetCore.Authentication.JwtBearer
- Scalar.AspNetCore
- Microsoft.AspNetCore.OpenApi

## 最佳实践

1. 认证配置：
   - 使用环境特定的认证服务器
   - 配置适当的作用域
   - 启用 HTTPS

2. API 文档：
   - 添加完整的 XML 注释
   - 提供请求和响应示例
   - 使用清晰的参数说明

3. 开发建议：
   - 开发环境启用详细日志
   - 使用环境变量覆盖配置
   - 遵循 RESTful 设计原则

## 注意事项

1. 开发环境：
   - 自动启用 OpenAPI 文档
   - 使用开发证书
   - 启用详细日志

2. 生产环境：
   - 禁用 OpenAPI 文档
   - 使用正式证书
   - 配置正确的认证服务器

3. 安全性：
   - 始终使用 HTTPS
   - 验证所有请求的认证信息
   - 适当配置 CORS 策略

## 相关项目

- [Tenon.AspNetCore.OpenApi.Extensions](../../src/Extensions/Tenon.AspNetCore.OpenApi.Extensions/README.md)

## 许可证

MIT 