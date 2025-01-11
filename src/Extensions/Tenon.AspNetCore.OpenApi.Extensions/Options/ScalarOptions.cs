using System.Text.Json.Serialization;

namespace Tenon.AspNetCore.OpenApi.Extensions.Options;

/// <summary>
/// Scalar UI 配置选项
/// </summary>
public class TenonScalarOptions
{
    /// <summary>
    /// 文档标题
    /// </summary>
    public string Title { get; set; } = "API Documentation";

    /// <summary>
    /// API 版本
    /// </summary>
    public string Version { get; set; } = "v1";

    /// <summary>
    /// API 描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 路由前缀
    /// </summary>
    public string RoutePrefix { get; set; } = "api-docs";

    /// <summary>
    /// 是否启用授权
    /// </summary>
    public bool EnableAuthorization { get; set; } = true;

    /// <summary>
    /// OAuth2 配置
    /// </summary>
    public OAuth2Options? OAuth2 { get; set; }

    /// <summary>
    /// 自定义样式配置
    /// </summary>
    public ScalarThemeOptions Theme { get; set; } = new();
}

/// <summary>
/// OAuth2 配置选项
/// </summary>
public class OAuth2Options
{
    /// <summary>
    /// 授权服务器地址
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// 客户端ID
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// 授权范围
    /// </summary>
    public List<string> Scopes { get; set; } = new();
}

/// <summary>
/// Scalar UI 主题配置
/// </summary>
public class ScalarThemeOptions
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
    public Dictionary<string, string> Colors { get; set; } = new()
    {
        { "primary", "#1976d2" },
        { "secondary", "#424242" },
        { "success", "#2e7d32" },
        { "error", "#d32f2f" },
        { "warning", "#ed6c02" },
        { "info", "#0288d1" }
    };
} 