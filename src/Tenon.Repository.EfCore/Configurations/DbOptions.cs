using System;
using System.ComponentModel.DataAnnotations;

namespace Tenon.Repository.EfCore.Configurations;

/// <summary>
/// 数据库配置选项
/// </summary>
public class DbOptions
{
    /// <summary>
    /// 默认命令超时时间（秒）
    /// </summary>
    public const int DefaultCommandTimeout = 30;

    /// <summary>
    /// 默认最大重试次数
    /// </summary>
    public const int DefaultMaxRetryCount = 3;

    /// <summary>
    /// 默认重试间隔（秒）
    /// </summary>
    public const int DefaultRetryInterval = 1;

    /// <summary>
    /// 数据库连接字符串
    /// </summary>
    [Required(ErrorMessage = "数据库连接字符串不能为空")]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// 命令超时时间（秒）
    /// </summary>
    /// <remarks>
    /// 默认值：30秒
    /// 如果设置为0，则使用数据库提供程序的默认超时时间
    /// </remarks>
    [Range(0, 3600, ErrorMessage = "命令超时时间必须在0-3600秒之间")]
    public int CommandTimeout { get; set; } = DefaultCommandTimeout;

    /// <summary>
    /// 是否启用详细错误信息
    /// </summary>
    /// <remarks>
    /// 建议仅在开发环境中启用
    /// </remarks>
    public bool EnableDetailedErrors { get; set; }

    /// <summary>
    /// 是否启用敏感数据日志
    /// </summary>
    /// <remarks>
    /// 建议仅在开发环境中启用
    /// </remarks>
    public bool EnableSensitiveDataLogging { get; set; }

    /// <summary>
    /// 最大重试次数
    /// </summary>
    /// <remarks>
    /// 默认值：3次
    /// 如果设置为0，则禁用重试机制
    /// </remarks>
    [Range(0, 10, ErrorMessage = "最大重试次数必须在0-10次之间")]
    public int MaxRetryCount { get; set; } = DefaultMaxRetryCount;

    /// <summary>
    /// 重试间隔（秒）
    /// </summary>
    /// <remarks>
    /// 默认值：1秒
    /// </remarks>
    [Range(1, 30, ErrorMessage = "重试间隔必须在1-30秒之间")]
    public int RetryInterval { get; set; } = DefaultRetryInterval;

    /// <summary>
    /// 验证配置是否有效
    /// </summary>
    /// <exception cref="ValidationException">当配置无效时抛出异常</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            throw new ValidationException("数据库连接字符串不能为空");
        }

        if (CommandTimeout < 0 || CommandTimeout > 3600)
        {
            throw new ValidationException("命令超时时间必须在0-3600秒之间");
        }

        if (MaxRetryCount < 0 || MaxRetryCount > 10)
        {
            throw new ValidationException("最大重试次数必须在0-10次之间");
        }

        if (RetryInterval < 1 || RetryInterval > 30)
        {
            throw new ValidationException("重试间隔必须在1-30秒之间");
        }
    }
}