using System.Net;
using FluentValidation;
using FluentValidation.Results;
using FluentValidationSample.Models;
using Microsoft.Extensions.Logging;
using Tenon.AspNetCore.Abstractions.Application;
using Tenon.FluentValidation.AspNetCore.Extensions;
using Tenon.FluentValidation.Extensions;

namespace FluentValidationSample.Services;

/// <summary>
/// 用户服务实现
/// </summary>
public class UserService : ServiceBase,IUserService
{
    private readonly IValidator<UserRegistrationRequest> _validator;
    private readonly ILogger<UserService> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="validator">验证器</param>
    /// <param name="logger">日志记录器</param>
    public UserService(IValidator<UserRegistrationRequest> validator, ILogger<UserService> logger)
    {
        _validator = validator;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<UserRegistrationResultDto>> RegisterAsync(UserRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // 验证请求
            ValidationResult? validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return validationResult.ToFluentValidationProblemDetails();

            // TODO: 实现实际的注册逻辑
            // 这里仅作示例，返回模拟数据
            var result = new UserRegistrationResultDto
            {
                UserId = Guid.NewGuid().ToString(),
                Username = request.Username,
                Email = request.Email,
                RegisterTime = DateTime.UtcNow
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用户注册失败: {Email}", request.Email);
            return Problem(HttpStatusCode.InternalServerError, "注册时发生错误");
        }
    }
} 