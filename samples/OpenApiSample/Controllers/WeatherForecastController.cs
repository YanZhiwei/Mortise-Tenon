using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenApiSample.Models;
using Tenon.AspNetCore.OpenApi.Extensions.ModelBinding;
using Microsoft.Extensions.Logging;

namespace OpenApiSample.Controllers;

/// <summary>
/// 天气预报控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "WeatherApiScope")]
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
    /// <param name="logger">日志记录器</param>
    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 获取天气预报
    /// </summary>
    /// <remarks>
    /// 此接口需要 OAuth2 认证，请在请求头中携带有效的 Bearer Token
    /// 
    /// 所需权限：
    /// - weather_api.read（天气预报读取权限）
    /// </remarks>
    /// <returns>天气预报列表</returns>
    /// <response code="200">成功获取天气预报</response>
    /// <response code="401">未授权</response>
    /// <response code="403">权限不足</response>
    [HttpGet]
    [Authorize(Policy = "WeatherApiReadScope")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

    /// <summary>
    /// 获取多个城市的天气预报
    /// </summary>
    /// <remarks>
    /// 此接口需要 OAuth2 认证，请在请求头中携带有效的 Bearer Token
    /// 
    /// 所需权限：
    /// - weather_api.write（天气预报写入权限）
    /// </remarks>
    /// <param name="request">天气预报请求</param>
    /// <returns>天气预报列表</returns>
    /// <response code="200">成功获取天气预报</response>
    /// <response code="401">未授权</response>
    /// <response code="403">权限不足</response>
    [HttpPost("batch")]
    [Authorize(Policy = "WeatherApiWriteScope")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IEnumerable<WeatherForecast> GetBatch([FromBody] WeatherForecastRequest request)
    {
        return request.Cities.SelectMany(city =>
            request.Days.Select(day => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(day)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = $"{city}: {Summaries[Random.Shared.Next(Summaries.Length)]}"
            }))
        .ToArray();
    }

    /// <summary>
    /// 获取天气预报统计信息
    /// </summary>
    /// <remarks>
    /// 此接口需要 OAuth2 认证，请在请求头中携带有效的 Bearer Token
    /// 
    /// 所需权限：
    /// - weather_api.admin（天气预报管理权限）
    /// </remarks>
    /// <returns>天气预报统计信息</returns>
    /// <response code="200">成功获取统计信息</response>
    /// <response code="401">未授权</response>
    /// <response code="403">权限不足</response>
    [HttpGet("stats")]
    [Authorize(Policy = "WeatherApiAdminScope")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<WeatherStats> GetStats()
    {
        return new WeatherStats
        {
            TotalRequests = 100,
            UniqueUsers = 10,
            LastUpdated = DateTime.UtcNow
        };
    }
}

/// <summary>
/// 天气预报模型
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

/// <summary>
/// 天气预报统计信息
/// </summary>
public class WeatherStats
{
    /// <summary>
    /// 总请求次数
    /// </summary>
    public int TotalRequests { get; set; }

    /// <summary>
    /// 唯一用户数
    /// </summary>
    public int UniqueUsers { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdated { get; set; }
} 