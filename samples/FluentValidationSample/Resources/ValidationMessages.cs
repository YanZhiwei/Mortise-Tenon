using System.Resources;

namespace FluentValidationSample.Resources;

/// <summary>
/// 验证消息资源标记类
/// </summary>
public class ValidationMessages
{
    private static readonly ResourceManager ResourceManager = new(
        "FluentValidationSample.Resources.ValidationMessages",
        typeof(ValidationMessages).Assembly);

    private ValidationMessages()
    {
    }

    public static string GetString(string name)
    {
        return ResourceManager.GetString(name) ?? name;
    }
} 