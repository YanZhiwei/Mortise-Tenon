using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OpenApiSample.Controllers;

/// <summary>
/// 天气预报控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "冰冻", "寒冷", "凉爽", "温和", "温暖", "炎热"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger">日志</param>
    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 获取天气预报
    /// </summary>
    /// <returns>天气预报列表</returns>
    [HttpGet]
    [Authorize]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
} 