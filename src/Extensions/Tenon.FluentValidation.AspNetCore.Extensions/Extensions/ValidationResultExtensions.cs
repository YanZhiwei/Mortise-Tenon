using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Diagnostics;

namespace Tenon.FluentValidation.AspNetCore.Extensions;

/// <summary>
/// ValidationResult 扩展方法
/// </summary>
public static class ValidationResultExtensions
{
    /// <summary>
    /// 将验证结果转换为本地化的 ValidationProblemDetails
    /// </summary>
    /// <param name="validationResult">验证结果</param>
    /// <param name="localizer">字符串本地化器</param>
    /// <returns>验证问题详情</returns>
    public static ActionResult ToLocalizedValidationProblemDetails(
        this ValidationResult validationResult,
        IStringLocalizer localizer)
    {
        if (validationResult.IsValid)
            return new OkResult();

        var errors = validationResult.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x =>
                {
                    // 尝试获取本地化消息
                    var localizedString = localizer[x.ErrorMessage];
                    Debug.WriteLine($"Localizing message: {x.ErrorMessage}");
                    Debug.WriteLine($"Resource found: {!localizedString.ResourceNotFound}");
                    Debug.WriteLine($"Localized value: {localizedString.Value}");

                    var localizedMessage = localizedString.ResourceNotFound ? x.ErrorMessage : localizedString.Value;

                    // 如果有占位符参数，进行格式化
                    if (x.FormattedMessagePlaceholderValues != null)
                    {
                        var formattedArgs = new List<object>();
                        var placeholderValues = x.FormattedMessagePlaceholderValues
                            .Where(kv => kv.Key != "PropertyName" && kv.Key != "PropertyValue")
                            .OrderBy(kv => kv.Key);

                        foreach (var kv in placeholderValues)
                        {
                            Debug.WriteLine($"Parameter {kv.Key}: {kv.Value}");
                            if (kv.Value != null)
                            {
                                formattedArgs.Add(kv.Value);
                            }
                        }

                        if (formattedArgs.Count > 0)
                        {
                            try
                            {
                                localizedMessage = string.Format(localizedMessage, formattedArgs.ToArray());
                                Debug.WriteLine($"Formatted message: {localizedMessage}");
                            }
                            catch (FormatException ex)
                            {
                                Debug.WriteLine($"Format error: {ex.Message}");
                            }
                        }
                    }

                    return localizedMessage;
                }).ToArray()
            );

        var problemDetails = new ValidationProblemDetails(errors)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Instance = "/api/User/register"
        };

        return new BadRequestObjectResult(problemDetails);
    }
} 