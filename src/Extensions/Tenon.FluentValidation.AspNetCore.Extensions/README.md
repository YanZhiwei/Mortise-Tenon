# Tenon.FluentValidation.AspNetCore.Extensions

ASP.NET Core 的 FluentValidation 扩展库，提供了便捷的验证和本地化支持。

## 功能特性

- 自动注册验证器
- 支持验证消息本地化
- 自定义验证响应格式
- 灵活的配置选项
- 内置常用验证扩展方法

## 安装

```bash
dotnet add package Tenon.FluentValidation.AspNetCore.Extensions
```

## 使用方法

1. 在 `Program.cs` 中注册服务：

```csharp
// 添加本地化支持
builder.Services.AddLocalization();

// 添加 FluentValidation
builder.Services.AddWebApiFluentValidation(
    builder.Configuration.GetSection("FluentValidation"),
    typeof(Program).Assembly);
```

2. 在 `appsettings.json` 中配置：

```json
{
  "FluentValidation": {
    "DisableDefaultModelValidation": true,
    "ValidatorLifetime": "Scoped"
  }
}
```

3. 创建验证器：

```csharp
public class UserRegistrationValidator : AbstractValidator<UserRegistrationRequest>
{
    public UserRegistrationValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.Username)
            .Required()
            .WithMessage(localizer["Username_Required"])
            .Length(3, 20)
            .WithMessage(localizer["Username_Length", 3, 20]);

        RuleFor(x => x.Email)
            .Required()
            .WithMessage(localizer["Email_Required"])
            .EmailAddress()
            .WithMessage(localizer["Email_Invalid"]);

        RuleFor(x => x.Password)
            .Required()
            .WithMessage(localizer["Password_Required"])
            .MinimumLength(6)
            .WithMessage(localizer["Password_Length", 6])
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$")
            .WithMessage(localizer["Password_Complexity"]);
    }
}
```

4. 添加本地化资源文件：

```
Resources/
  ├── ValidationMessages.cs          # 资源标记类
  └── ValidationMessages.zh-CN.resx  # 中文资源文件
```

资源文件示例（ValidationMessages.zh-CN.resx）：
```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="Username_Required" xml:space="preserve">
    <value>请输入用户名</value>
  </data>
  <data name="Username_Length" xml:space="preserve">
    <value>用户名长度必须在 {0} 到 {1} 个字符之间</value>
  </data>
  <data name="Email_Required" xml:space="preserve">
    <value>请输入电子邮件地址</value>
  </data>
  <data name="Email_Invalid" xml:space="preserve">
    <value>请输入有效的电子邮件地址</value>
  </data>
</root>
```

## 配置选项

| 选项 | 说明 | 默认值 |
|------|------|--------|
| DisableDefaultModelValidation | 是否禁用 ASP.NET Core 默认的模型验证响应 | true |
| ValidatorLifetime | 验证器生命周期 | Scoped |

## 验证器扩展方法

| 方法 | 说明 | 示例 |
|------|------|------|
| Required | 必填验证 | `.Required()` |
| Length | 长度验证 | `.Length(3, 20)` |
| MinimumLength | 最小长度验证 | `.MinimumLength(6)` |
| Matches | 正则表达式验证 | `.Matches(@"^(?=.*[a-z])(?=.*[A-Z]).*$")` |

## 本地化支持

支持通过资源文件进行验证消息的本地化，可以通过以下方式指定语言：

1. 查询字符串：`?culture=zh-CN`
2. Cookie：`c=zh-CN|uic=zh-CN`
3. Accept-Language 头：`Accept-Language: zh-CN`

## 示例项目

可以在 [samples/FluentValidationSample](../../samples/FluentValidationSample) 中查看完整的示例项目。 