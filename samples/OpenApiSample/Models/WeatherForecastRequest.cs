using System.ComponentModel.DataAnnotations;

namespace OpenApiSample.Models;

/// <summary>
/// 天气预报请求
/// </summary>
public class WeatherForecastRequest
{
    /// <summary>
    /// 城市列表
    /// </summary>
    /// <example>["北京", "上海", "广州"]</example>
    [Required]
    public string[] Cities { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 天数列表
    /// </summary>
    /// <example>[1, 2, 3]</example>
    [Required]
    public int[] Days { get; set; } = Array.Empty<int>();
} 