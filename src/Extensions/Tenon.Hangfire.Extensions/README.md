# Tenon.Hangfire.Extensions

## 项目说明
Tenon.Hangfire.Extensions 是基于 Hangfire 的任务调度扩展库，提供了更易用和更安全的任务调度功能。

## 功能特性
- 基本认证支持
- IP 白名单控制
- 仪表板配置
- 任务调度管理

## 优化计划

### 1. 安全性优化
- [ ] 增强密码认证机制
  - [ ] 添加密码复杂度验证
  - [ ] 实现登录失败次数限制
  - [ ] 支持 JWT/OAuth2 认证
- [ ] 改进 IP 白名单管理
  - [ ] 实现配置化管理
  - [ ] 支持 IP 范围配置
  - [ ] 添加动态 IP 管理

### 2. 配置系统优化
- [ ] 扩展配置选项
  - [ ] 任务重试策略配置
  - [ ] 任务超时配置
  - [ ] 存储配置选项
  - [ ] 并发任务限制

### 3. 架构优化
- [ ] 添加核心抽象
  - [ ] 任务执行抽象
  - [ ] 异常处理机制
  - [ ] 状态监控接口
  - [ ] 日志记录标准

### 4. 性能优化
- [ ] 性能监控
  - [ ] 指标收集
  - [ ] 执行追踪
  - [ ] 队列监控

### 5. 功能扩展
- [ ] 高级特性
  - [ ] 任务调度模板
  - [ ] 分布式锁
  - [ ] 优先级管理
  - [ ] 依赖关系管理

### 6. 可用性提升
- [ ] 运维支持
  - [ ] 健康检查
  - [ ] 优雅关闭
  - [ ] 审计日志
  - [ ] 报警机制

## 使用说明

### 安装
```csharp
dotnet add package Tenon.Hangfire.Extensions
```

### 基本配置
```csharp
// 在 appsettings.json 中配置
{
  "Hangfire": {
    "Username": "admin",
    "Password": "your-secure-password",
    "Path": "/hangfire",
    "DashboardTitle": "任务调度中心"
  }
}

// 在 Program.cs 中使用
app.UseHangfire(Configuration.GetSection("Hangfire"));
```

## 贡献指南
欢迎提交 Issue 和 Pull Request 来帮助改进项目。

## 开源协议
MIT 