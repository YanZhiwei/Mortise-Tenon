using Microsoft.AspNetCore.Http;

namespace Tenon.AspNetCore.Filters.Validators;

public class FileNameValidator : IFileValidator
{
    public FileValidationResult Validate(IFormFile file)
    {
        var fileName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(fileName))
            return FileValidationResult.InvalidFileName("File name cannot be empty");

        if (fileName.Any(c => Path.GetInvalidFileNameChars().Contains(c)))
            return FileValidationResult.InvalidFileName("File name contains invalid characters");

        return FileValidationResult.Success();
    }
} 