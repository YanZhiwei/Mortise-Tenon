using Microsoft.AspNetCore.Http;

namespace Tenon.AspNetCore.Filters.Validators;

public interface IFileValidator
{
    FileValidationResult Validate(IFormFile file);
} 