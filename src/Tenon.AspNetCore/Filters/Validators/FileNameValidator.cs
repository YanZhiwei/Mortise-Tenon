using Microsoft.AspNetCore.Http;

namespace Tenon.AspNetCore.Filters.Validators;

public class FileNameValidator : IFileValidator
{
    public FileValidationResult Validate(IFormFile file)
    {
        var fileName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(fileName))
            return FileValidationResult.InvalidFileName("文件名不能为空");

        if (fileName.Any(c => Path.GetInvalidFileNameChars().Contains(c)))
            return FileValidationResult.InvalidFileName("文件名包含非法字符");

        return FileValidationResult.Success();
    }
} 