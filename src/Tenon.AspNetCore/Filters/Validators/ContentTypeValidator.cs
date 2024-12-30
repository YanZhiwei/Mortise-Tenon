using Microsoft.AspNetCore.Http;

namespace Tenon.AspNetCore.Filters.Validators;

public class ContentTypeValidator : IFileValidator
{
    private readonly HashSet<string> _allowedContentTypes;

    public ContentTypeValidator(HashSet<string> allowedContentTypes)
    {
        _allowedContentTypes = allowedContentTypes;
    }

    public FileValidationResult Validate(IFormFile file)
    {
        return _allowedContentTypes.Contains(file.ContentType)
            ? FileValidationResult.Success()
            : FileValidationResult.UnsupportedContentType(file.ContentType);
    }
} 