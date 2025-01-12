using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tenon.AspNetCore.OpenApi.Extensions.Configurations;

/// <summary>
/// Scalar UI 配置选项
/// </summary>
public class ScalarUIOptions : IValidatableObject
{
    /// <summary>
    /// 文档标题
    /// </summary>
    [Required(ErrorMessage = "文档标题不能为空")]
    [StringLength(100, ErrorMessage = "文档标题长度不能超过100个字符")]
    public string Title { get; set; } = "API Documentation";

    /// <summary>
    /// API 版本
    /// </summary>
    [Required(ErrorMessage = "API版本不能为空")]
    [RegularExpression(@"^v\d+(\.\d+)*$", ErrorMessage = "API版本格式不正确，应为v1、v1.0等格式")]
    public string Version { get; set; } = "v1";

    /// <summary>
    /// API 描述
    /// </summary>
    [StringLength(500, ErrorMessage = "API描述长度不能超过500个字符")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 主题配置
    /// </summary>
    [Required(ErrorMessage = "主题配置不能为空")]
    public ScalarThemeOptions Theme { get; set; } = new();

    /// <summary>
    /// 验证配置
    /// </summary>
    /// <param name="validationContext">验证上下文</param>
    /// <returns>验证结果</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        yield break;
    }
}

/// <summary>
/// Scalar UI 主题配置
/// </summary>
public class ScalarThemeOptions : IValidatableObject
{
    /// <summary>
    /// 是否启用暗色主题
    /// </summary>
    [JsonPropertyName("darkMode")]
    public bool DarkMode { get; set; }

    /// <summary>
    /// 主题色
    /// </summary>
    [JsonPropertyName("colors")]
    [Required(ErrorMessage = "主题颜色配置不能为空")]
    public Dictionary<string, string> Colors { get; set; } = new()
    {
        { "primary", "#1976d2" },
        { "secondary", "#424242" },
        { "success", "#2e7d32" },
        { "error", "#d32f2f" },
        { "warning", "#ed6c02" },
        { "info", "#0288d1" }
    };

    /// <summary>
    /// 验证配置
    /// </summary>
    /// <param name="validationContext">验证上下文</param>
    /// <returns>验证结果</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var requiredColors = new[] { "primary", "secondary", "success", "error", "warning", "info" };
        var missingColors = requiredColors.Except(Colors.Keys);

        if (missingColors.Any())
        {
            yield return new ValidationResult(
                $"缺少必需的主题颜色：{string.Join(", ", missingColors)}",
                new[] { nameof(Colors) }
            );
        }

        var invalidColors = Colors.Values.Where(color => !IsValidHexColor(color));
        if (invalidColors.Any())
        {
            yield return new ValidationResult(
                "颜色值必须是有效的十六进制颜色代码（如 #1976d2）",
                new[] { nameof(Colors) }
            );
        }
    }

    private static bool IsValidHexColor(string color)
    {
        return !string.IsNullOrEmpty(color) && 
               System.Text.RegularExpressions.Regex.IsMatch(color, @"^#[0-9A-Fa-f]{6}$");
    }
} 