# FileUploadSample

这是一个演示如何使用 Tenon.AspNetCore 中的文件上传功能的示例项目。该示例展示了如何配置和使用文件上传验证、本地化支持以及外部资源文件上传功能。

## 功能特点

1. 文件上传验证
   - 文件大小限制
   - 文件类型限制
   - 文件名验证
   - 外部资源文件支持

2. 本地化支持
   - 支持中文和英文
   - 可扩展支持其他语言
   - 错误消息本地化

3. API 接口
   - 单文件上传
   - 多文件上传
   - 外部资源文件上传

## 配置说明

### 1. 文件上传配置

在 `appsettings.json` 中配置文件上传选项：

```json
{
  "FileUploadSettings": {
    "MaxRequestBodySize": 104857600,  // 最大请求体大小（100MB）
    "MaxFileSize": 10485760,          // 单个文件最大大小（10MB）
    "AllowedExtensions": [            // 允许的文件扩展名
      ".jpg",
      ".png",
      ".pdf"
    ],
    "ValidateFileName": true,         // 是否验证文件名
    "AllowExternalResources": true,   // 是否允许外部资源文件
    "ExternalResourceTimeout": "00:00:30", // 外部资源下载超时时间
    "AllowedExternalDomains": [       // 允许的外部资源域名
      "example.com",
      "images.example.com"
    ]
  }
}
```

### 2. 本地化配置

本地化资源文件位于 `Resources` 目录：
- `FileValidation.en.resx`：英文资源文件
- `FileValidation.zh.resx`：中文资源文件

要添加新的语言支持，只需要：
1. 在 `Resources` 目录添加对应的资源文件（如 `FileValidation.fr.resx`）
2. 在 `Program.cs` 中的 `supportedCultures` 数组添加新的语言文化信息

## API 使用说明

### 1. 单文件上传

```http
POST /api/file/upload
Content-Type: multipart/form-data

file: (binary)
```

### 2. 多文件上传

```http
POST /api/file/upload-multiple
Content-Type: multipart/form-data

files: (binary)
```

### 3. 外部资源文件上传

```http
POST /api/file/upload-external
Content-Type: multipart/form-data

url: https://example.com/image.jpg
```

## 本地化测试

要测试不同语言的错误消息，可以在请求头中设置 `Accept-Language`：

```http
// 英文
Accept-Language: en

// 中文
Accept-Language: zh
```

## 运行项目

1. 确保已安装 .NET 9.0 SDK
2. 克隆仓库
3. 进入项目目录：`cd samples/FileUploadSample`
4. 运行项目：`dotnet run`
5. 访问 Swagger UI：`https://localhost:5001/swagger`

## 注意事项

1. 上传的文件保存在项目根目录的 `uploads` 文件夹中
2. 文件名会自动生成为 GUID，以避免文件名冲突
3. 外部资源文件上传时会自动下载并保存为本地文件
4. 请确保配置文件中的文件大小限制符合你的需求
5. 生产环境中建议配置更安全的文件存储位置 