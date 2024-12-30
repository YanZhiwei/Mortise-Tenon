using Microsoft.AspNetCore.Http;

namespace Tenon.AspNetCore.Filters.Validators;

public class FileSizeValidator : IFileValidator
{
    private readonly long _maxFileSize;

    public FileSizeValidator(long maxFileSize)
    {
        _maxFileSize = maxFileSize;
    }

    public FileValidationResult Validate(IFormFile file)
    {
        return file.Length <= _maxFileSize
            ? FileValidationResult.Success()
            : FileValidationResult.FileTooLarge(_maxFileSize);
    }
} 