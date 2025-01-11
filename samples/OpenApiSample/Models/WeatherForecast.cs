namespace OpenApiSample.Models;

/// <summary>
/// 天气预报
/// </summary>
public class WeatherForecast
{
    /// <summary>
    /// 日期
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// 摄氏温度
    /// </summary>
    public int TemperatureC { get; set; }

    /// <summary>
    /// 华氏温度
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    /// <summary>
    /// 天气概况
    /// </summary>
    public string? Summary { get; set; }
} 