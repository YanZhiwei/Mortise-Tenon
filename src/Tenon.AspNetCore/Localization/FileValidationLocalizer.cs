using Microsoft.Extensions.Localization;

namespace Tenon.AspNetCore.Localization;

/// <summary>
/// 文件验证本地化服务
/// </summary>
public class FileValidationLocalizer
{
    private readonly IStringLocalizer _localizer;

    public FileValidationLocalizer(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create(typeof(FileValidationLocalizer));
    }

    public LocalizedString GetLocalizedString(string name)
    {
        return _localizer[name];
    }

    public LocalizedString GetLocalizedString(string name, params object[] arguments)
    {
        return _localizer[name, arguments];
    }
} 