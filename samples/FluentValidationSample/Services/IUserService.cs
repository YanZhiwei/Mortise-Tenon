using FluentValidationSample.Models;
using Tenon.AspNetCore.Abstractions.Application;

namespace FluentValidationSample.Services;

/// <summary>
/// 用户服务接口
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="request">注册请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>注册结果</returns>
    Task<ServiceResult<UserRegistrationResultDto>> RegisterAsync(UserRegistrationRequest request, CancellationToken cancellationToken);
} 