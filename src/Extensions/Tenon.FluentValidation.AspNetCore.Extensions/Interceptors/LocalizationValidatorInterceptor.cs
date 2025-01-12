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
    private readonly IStringLocalizerFactory _localizerFactory;

    /// <summary>
    /// 初始化本地化验证拦截器
    /// </summary>
    /// <param name="localizerFactory">字符串本地化器工厂</param>
    public LocalizationValidatorInterceptor(IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
    }

    /// <summary>
    /// 验证前处理
    /// </summary>
    /// <param name="actionContext">操作上下文</param>
    /// <param name="validationContext">验证上下文</param>
    /// <returns>验证上下文</returns>
    public IValidationContext BeforeAspNetValidation(ActionContext actionContext, IValidationContext validationContext)
    {
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
        if (!result.IsValid)
        {
            // 获取验证消息的本地化器
            var validationType = validationContext.InstanceToValidate.GetType();
            var resourceType = Type.GetType($"{validationType.Namespace}.Resources.ValidationMessages, {validationType.Assembly.GetName().Name}");
            
            // 如果找不到资源类型，则使用默认的本地化器
            var localizer = resourceType != null
                ? _localizerFactory.Create(resourceType)
                : _localizerFactory.Create(typeof(LocalizationValidatorInterceptor));

            // 本地化错误消息
            var errors = result.Errors.Select(error =>
            {
                // 尝试获取本地化消息
                var localizedString = localizer[error.ErrorMessage];
                var localizedMessage = localizedString.ResourceNotFound ? error.ErrorMessage : localizedString.Value;

                // 如果有占位符参数，进行格式化
                if (error.FormattedMessagePlaceholderValues != null)
                {
                    var args = error.FormattedMessagePlaceholderValues
                        .Where(x => x.Key != "PropertyName" && x.Key != "PropertyValue")
                        .Select(x => x.Value)
                        .ToArray();
                    
                    if (args.Length > 0)
                    {
                        localizedMessage = string.Format(localizedMessage, args);
                    }
                }

                return new ValidationFailure(error.PropertyName, localizedMessage)
                {
                    ErrorCode = error.ErrorCode,
                    FormattedMessagePlaceholderValues = error.FormattedMessagePlaceholderValues,
                    PropertyName = error.PropertyName,
                    Severity = error.Severity,
                    AttemptedValue = error.AttemptedValue,
                    CustomState = error.CustomState
                };
            }).ToList();

            return new ValidationResult(errors);
        }

        return result;
    }
} 