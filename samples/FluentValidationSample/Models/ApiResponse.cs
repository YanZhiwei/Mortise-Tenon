namespace FluentValidationSample.Models;

/// <summary>
/// API 响应模型
/// </summary>
/// <typeparam name="T">响应数据类型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 数据
    /// </summary>
    public T? Data { get; set; }
}

/// <summary>
/// 验证错误响应模型
/// </summary>
public class ValidationErrorResponse
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 验证错误列表
    /// </summary>
    public IEnumerable<ValidationError> Errors { get; set; } = Array.Empty<ValidationError>();
}

/// <summary>
/// 验证错误详情
/// </summary>
public class ValidationError
{
    /// <summary>
    /// 属性名
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// 错误消息
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
} 