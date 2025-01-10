# Hangfire 示例项目

这是一个演示如何使用 Tenon.Hangfire.Extensions 的示例项目。

## 功能特性

- 基本认证和 IP 白名单保护
- 密码复杂度验证
- 登录失败次数限制
- 支持多种任务类型：
  - 即时任务
  - 延迟任务
  - 定时任务
  - 连续任务
  - 带参数任务
  - 长时间运行任务
- 使用 SQLite 数据库，无需额外依赖

## 快速开始

1. 克隆项目
2. 运行项目（SQLite 数据库会自动创建）
3. 访问 Swagger UI：https://localhost:5001/swagger
4. 访问 Hangfire 仪表板：https://localhost:5001/hangfire
   - 用户名：admin
   - 密码：Admin@123

## API 端点

- POST /api/job/fire-and-forget - 创建即时任务
- POST /api/job/delayed?delayInSeconds=30 - 创建延迟任务
- POST /api/job/recurring?cronExpression=*/5 * * * * - 创建定时任务
- POST /api/job/continuation - 创建连续任务
- POST /api/job/parameterized - 创建带参数任务
- POST /api/job/long-running - 创建长时间运行任务
- POST /api/job/failable - 创建可能失败的任务

## 配置说明

```json
{
  "ConnectionStrings": {
    "HangfireConnection": "Data Source=hangfire.db"
  },
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

## 示例任务

项目包含以下示例任务：

1. 简单任务：
   - 执行基本的控制台输出
   - 模拟 5 秒的执行时间

2. 带参数任务：
   - 支持传入自定义参数
   - 记录参数信息到日志

3. 长时间运行任务：
   - 支持指定运行时长
   - 定期报告执行进度

4. 可能失败的任务：
   - 演示任务失败的情况
   - 用于测试重试机制

## 性能优化

1. 工作线程配置：
   - 工作线程数：CPU 核心数 * 2
   - 支持多个任务队列（default、critical）
   - 可配置的服务器超时和关闭超时

2. SQLite 配置：
   - 使用文件数据库，便于部署
   - 可配置的轮询间隔
   - 支持并发访问

## 注意事项

1. 在生产环境中：
   - 修改默认的用户名和密码
   - 配置适当的 IP 白名单
   - 启用 HTTPS
   - 考虑使用更强大的数据库（如 SQL Server、PostgreSQL）

2. 安全建议：
   - 定期更改密码
   - 限制访问 IP 范围
   - 监控登录失败记录
   - 及时更新依赖包

## 常见问题

1. 数据库访问问题：
   - 确保应用有写入权限
   - 检查 SQLite 数据库文件路径
   - 注意并发访问限制

2. 访问权限问题：
   - 检查用户名和密码是否正确
   - 确认当前 IP 是否在白名单中
   - 查看是否被登录失败锁定

## 贡献指南

欢迎提交 Issue 和 Pull Request 来帮助改进项目。 