using FluentValidation;
using FluentValidationSample.Models;
using FluentValidationSample.Resources;
using FluentValidationSample.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Tenon.AspNetCore.Controllers;
using Tenon.FluentValidation.AspNetCore.Extensions;

namespace FluentValidationSample.Controllers;

/// <summary>
/// 用户控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserController : AbstractController
{
    private readonly IValidator<UserRegistrationRequest> _validator;
    private readonly IUserService _userService;
    private readonly IStringLocalizer<ValidationMessages> _localizer;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="validator">验证器</param>
    /// <param name="userService">用户服务</param>
    /// <param name="localizer">本地化器</param>
    public UserController(
        IValidator<UserRegistrationRequest> validator,
        IUserService userService,
        IStringLocalizer<ValidationMessages> localizer)
    {
        _validator = validator;
        _userService = userService;
        _localizer = localizer;
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
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserRegistrationResultDto>> Register(
        [FromBody] UserRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        // 手动验证并本地化错误消息
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        
        // 测试本地化器是否正常工作
        var testMessage = _localizer["Username_Required"];
        System.Diagnostics.Debug.WriteLine($"Test localization - ResourceNotFound: {testMessage.ResourceNotFound}, Value: {testMessage.Value}");
        
        if (!validationResult.IsValid)
            return validationResult.ToLocalizedValidationProblemDetails(_localizer);

        var result = await _userService.RegisterAsync(request, cancellationToken);
        return Result(result);
    }
}