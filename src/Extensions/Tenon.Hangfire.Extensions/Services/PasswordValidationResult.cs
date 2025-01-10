using System.Collections.Generic;

namespace Tenon.Hangfire.Extensions.Services;

/// <summary>
/// 密码验证结果
/// </summary>
public class PasswordValidationResult
{
    /// <summary>
    /// 获取验证是否通过
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// 获取验证错误信息列表
    /// </summary>
    public List<string> Errors { get; } = new();

    /// <summary>
    /// 添加错误信息
    /// </summary>
    /// <param name="error">错误信息</param>
    public void AddError(string error)
    {
        if (!string.IsNullOrEmpty(error))
        {
            Errors.Add(error);
        }
    }
} 