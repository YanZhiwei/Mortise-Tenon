using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Tenon.FluentValidation.AspNetCore.Extensions.Interceptors;

/// <summary>
/// 本地化验证拦截器
/// </summary>
public class LocalizationValidatorInterceptor : IValidatorInterceptor
{
    private readonly IStringLocalizer _localizer;

    /// <summary>
    /// 初始化本地化验证拦截器
    /// </summary>
    /// <param name="localizer">字符串本地化器</param>
    public LocalizationValidatorInterceptor(IStringLocalizer<LocalizationValidatorInterceptor> localizer)
    {
        _localizer = localizer;
    }

    /// <summary>
    /// 验证前处理
    /// </summary>
    /// <param name="actionContext">操作上下文</param>
    /// <param name="validationContext">验证上下文</param>
    /// <returns>验证上下文</returns>
    public IValidationContext BeforeAspNetValidation(ActionContext actionContext, IValidationContext validationContext)
    {
        // 在验证之前，可以修改验证上下文
        return validationContext;
    }

    /// <summary>
    /// 验证后处理
    /// </summary>
    /// <param name="actionContext">操作上下文</param>
    /// <param name="validationContext">验证上下文</param>
    /// <param name="result">验证结果</param>
    /// <returns>处理后的验证结果</returns>
    public ValidationResult AfterAspNetValidation(ActionContext actionContext, IValidationContext validationContext,
        ValidationResult result)
    {
        // 在验证之后，可以修改验证结果
        if (!result.IsValid)
        {
            // 本地化错误消息
            var errors = result.Errors.Select(error => new ValidationFailure(error.PropertyName,
                _localizer[error.ErrorMessage].Value));

            result = new ValidationResult(errors);
        }

        return result;
    }
} 