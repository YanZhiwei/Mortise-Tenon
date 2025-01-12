# FluentValidation Sample

这是一个演示如何在 ASP.NET Core 项目中使用 Tenon.FluentValidation.AspNetCore.Extensions 的示例项目。

## 项目结构

```
FluentValidationSample/
  ├── Controllers/           # 控制器
  ├── Models/               # 数据模型
  ├── Resources/            # 本地化资源
  ├── Services/             # 业务服务
  ├── Validators/           # 验证器
  ├── appsettings.json      # 配置文件
  └── Program.cs            # 程序入口
```

## 功能演示

1. **用户注册验证**
   - 用户名验证
     - 必填
     - 长度在 3-20 个字符之间
   - 电子邮件验证
     - 必填
     - 必须是有效的电子邮件格式
   - 密码验证
     - 必填
     - 最小长度 6 个字符
     - 必须包含大小写字母、数字和特殊字符
   - 确认密码验证
     - 必填
     - 必须与密码相同
   - 年龄验证
     - 必填
     - 必须大于或等于 18 岁

2. **本地化支持**
   - 中文验证消息
   - 支持多语言切换
   - 支持参数化消息

## 验证规则示例

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

## 运行项目

1. 确保已安装 .NET 9.0 SDK
2. 克隆仓库
3. 进入项目目录
4. 运行项目：

```bash
dotnet run
```

## API 接口

### 用户注册

```http
POST /api/User/register
Content-Type: application/json

{
  "username": "test",
  "email": "test@example.com",
  "password": "Test123!",
  "confirmPassword": "Test123!",
  "age": 18
}
```

### 验证错误响应示例

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Username": [
      "请输入用户名",
      "用户名长度必须在 3 到 20 个字符之间"
    ],
    "Password": [
      "密码必须包含至少一个大写字母、一个小写字母、一个数字和一个特殊字符"
    ]
  }
}
```

## 本地化测试

1. 使用查询字符串：
```
POST /api/User/register?culture=zh-CN
```

2. 使用 Accept-Language 头：
```
Accept-Language: zh-CN
```

## 配置说明

`appsettings.json` 中的主要配置：

```json
{
  "FluentValidation": {
    "DisableDefaultModelValidation": true,
    "ValidatorLifetime": "Scoped"
  },
  "Localization": {
    "DefaultCulture": "zh-CN",
    "SupportedCultures": ["zh-CN", "en-US"]
  }
}
``` 