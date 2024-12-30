using Microsoft.AspNetCore.Http;

namespace Tenon.AspNetCore.Filters.Validators;

public class FileExtensionValidator(HashSet<string> allowedExtensions) : IFileValidator
{
    public FileValidationResult Validate(IFormFile file)
    {
        if (allowedExtensions.Count == 0) return FileValidationResult.Success();

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension)
            ? FileValidationResult.Success()
            : FileValidationResult.UnsupportedExtension(extension);
    }
} 