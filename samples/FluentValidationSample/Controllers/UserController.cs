using FluentValidationSample.Models;
using Microsoft.AspNetCore.Mvc;

namespace FluentValidationSample.Controllers;

/// <summary>
/// 用户控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="request">注册请求</param>
    /// <returns>注册结果</returns>
    [HttpPost("register")]
    public IActionResult Register([FromBody] UserRegistrationRequest request)
    {
        // 如果验证通过，返回成功
        return Ok(new { Message = "注册成功", Data = request });
    }
} 