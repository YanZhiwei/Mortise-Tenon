using System.ComponentModel.DataAnnotations;

namespace Tenon.FluentValidation.AspNetCore.Extensions.Options;

/// <summary>
/// 本地化配置选项
/// </summary>
public class LocalizationOptions
{
    /// <summary>
    /// 资源文件路径
    /// </summary>
    [Required]
    public string ResourcesPath { get; set; } = "Resources";

    /// <summary>
    /// 默认文化
    /// </summary>
    [Required]
    public string DefaultCulture { get; set; } = "zh-CN";

    /// <summary>
    /// 支持的文化列表
    /// </summary>
    [Required]
    public string[] SupportedCultures { get; set; } = { "zh-CN" };
} 