# Tenon.Infra.Swagger

## 介绍
Tenon.Infra.Swagger 是一个用于 ASP.NET Core 应用程序的扩展库，旨在简化 Swagger 的集成和配置，特别是与 JWT 认证的结合。该库提供了便捷的方法来添加 Swagger Bearer 授权头，使得 API 文档能够清晰地展示如何使用 JWT 进行身份验证。

## 示例代码
以下是如何在 ASP.NET Core 项目中使用 Tenon.Infra.Swagger 的示例代码：

```csharp
using Microsoft.Extensions.DependencyInjection;
using Tenon.Infra.Swagger.Extensions;
public class Startup
{
public void ConfigureServices(IServiceCollection services)
{
// 添加 Swagger Bearer 授权头
services.AddSwaggerBearerAuthorizationHeader();
// 其他服务配置...
}
// 其他方法...
}
```

通过调用 `AddSwaggerBearerAuthorizationHeader` 方法，您可以轻松地将 JWT 认证集成到 Swagger 文档中，确保 API 用户能够理解如何进行身份验证。