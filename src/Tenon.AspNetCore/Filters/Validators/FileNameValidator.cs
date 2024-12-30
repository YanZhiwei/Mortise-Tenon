using Microsoft.AspNetCore.Http;
using Tenon.AspNetCore.Resources;

namespace Tenon.AspNetCore.Filters.Validators;

public class FileNameValidator : IFileValidator
{
    public FileValidationResult Validate(IFormFile file)
    {
        var fileName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(fileName))
            return FileValidationResult.InvalidFileName(FileValidationResource.FileNameEmpty);

        if (fileName.Any(c => Path.GetInvalidFileNameChars().Contains(c)))
            return FileValidationResult.InvalidFileName(FileValidationResource.FileNameInvalidChars);

        return FileValidationResult.Success();
    }
} 