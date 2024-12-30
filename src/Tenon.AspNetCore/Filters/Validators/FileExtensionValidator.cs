using Microsoft.AspNetCore.Http;

namespace Tenon.AspNetCore.Filters.Validators;

public class FileExtensionValidator : IFileValidator
{
    private readonly HashSet<string> _allowedExtensions;

    public FileExtensionValidator(HashSet<string> allowedExtensions)
    {
        _allowedExtensions = allowedExtensions;
    }

    public FileValidationResult Validate(IFormFile file)
    {
        if (_allowedExtensions.Count == 0) return FileValidationResult.Success();

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return _allowedExtensions.Contains(extension)
            ? FileValidationResult.Success()
            : FileValidationResult.UnsupportedExtension(extension);
    }
} 