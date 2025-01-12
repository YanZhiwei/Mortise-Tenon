using FluentValidationSample.Models;
using FluentValidationSample.Services;
using Microsoft.AspNetCore.Mvc;
using Tenon.AspNetCore.Controllers;

namespace FluentValidationSample.Controllers;

/// <summary>
/// 用户控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserController : AbstractController
{
    private readonly IUserService _userService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="userService">用户服务</param>
    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="request">注册请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>注册结果</returns>
    /// <response code="200">注册成功</response>
    /// <response code="400">请求参数验证失败</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserRegistrationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserRegistrationResultDto>> Register(
        [FromBody] UserRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _userService.RegisterAsync(request, cancellationToken);
        return Result(result);
    }
}