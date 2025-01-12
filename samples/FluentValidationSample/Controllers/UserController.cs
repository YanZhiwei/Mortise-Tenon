using FluentValidation;
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
    private readonly IValidator<UserRegistrationRequest> _validator;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="validator">用户注册请求验证器</param>
    public UserController(IValidator<UserRegistrationRequest> validator)
    {
        _validator = validator;
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="request">注册请求</param>
    /// <returns>注册结果</returns>
    /// <response code="200">注册成功</response>
    /// <response code="400">请求参数验证失败</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<UserRegistrationRequest>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
    {
        // 显式调用验证器
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ValidationErrorResponse
            {
                Success = false,
                Message = "验证失败",
                Errors = validationResult.Errors.Select(error => new ValidationError
                {
                    PropertyName = error.PropertyName,
                    ErrorMessage = error.ErrorMessage
                })
            });
        }

        // 如果验证通过，返回成功
        return Ok(new ApiResponse<UserRegistrationRequest>
        {
            Success = true,
            Message = "注册成功",
            Data = request
        });
    }
} 