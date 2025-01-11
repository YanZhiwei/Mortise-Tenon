using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tenon.AspNetCore.OpenApi.Extensions.Options;

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
    /// OAuth2 配置
    /// </summary>
    public OAuth2Options? OAuth2 { get; set; }

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
        // 如果启用了 OAuth2，则必须提供相关配置
        if (OAuth2 != null)
        {
            if (string.IsNullOrEmpty(OAuth2.Authority))
            {
                yield return new ValidationResult(
                    "启用 OAuth2 时，Authority 不能为空",
                    new[] { nameof(OAuth2.Authority) }
                );
            }

            if (string.IsNullOrEmpty(OAuth2.ClientId))
            {
                yield return new ValidationResult(
                    "启用 OAuth2 时，ClientId 不能为空",
                    new[] { nameof(OAuth2.ClientId) }
                );
            }

            if (OAuth2.Scopes == null || !OAuth2.Scopes.Any())
            {
                yield return new ValidationResult(
                    "启用 OAuth2 时，至少需要一个 Scope",
                    new[] { nameof(OAuth2.Scopes) }
                );
            }
        }
    }
}

/// <summary>
/// OAuth2 配置选项
/// </summary>
public class OAuth2Options : IValidatableObject
{
    /// <summary>
    /// 授权服务器地址
    /// </summary>
    [Url(ErrorMessage = "授权服务器地址必须是有效的URL")]
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// 客户端ID
    /// </summary>
    [Required(ErrorMessage = "客户端ID不能为空")]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// 授权范围
    /// </summary>
    public List<string> Scopes { get; set; } = new();

    /// <summary>
    /// 验证配置
    /// </summary>
    /// <param name="validationContext">验证上下文</param>
    /// <returns>验证结果</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Scopes.Any(scope => string.IsNullOrWhiteSpace(scope)))
        {
            yield return new ValidationResult(
                "Scope 不能为空",
                new[] { nameof(Scopes) }
            );
        }
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