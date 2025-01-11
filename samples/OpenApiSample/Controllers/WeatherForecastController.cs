using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenApiSample.Models;
using Tenon.AspNetCore.OpenApi.Extensions.ModelBinding;

namespace OpenApiSample.Controllers;

/// <summary>
/// 天气预报控制器
/// </summary>
[ApiController]
[Route("[controller]")]
[Authorize(Policy = "WeatherApiScope")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    /// <summary>
    /// 获取天气预报
    /// </summary>
    /// <param name="days">天数（逗号分隔的数组，例如：1,2,3）</param>
    /// <returns>天气预报列表</returns>
    [HttpGet]
    public IEnumerable<WeatherForecast> Get([FromQuery(Name = "days")] [ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] days)
    {
        return Enumerable.Range(1, days.Length).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(days[index - 1])),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    /// <summary>
    /// 获取多个城市的天气预报
    /// </summary>
    /// <param name="request">天气预报请求</param>
    /// <returns>天气预报列表</returns>
    [HttpPost("batch")]
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
} 