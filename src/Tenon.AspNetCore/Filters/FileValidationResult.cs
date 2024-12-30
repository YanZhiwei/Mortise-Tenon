using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Tenon.AspNetCore.Localization;
using Tenon.AspNetCore.Resources;

namespace Tenon.AspNetCore.Filters;

public class FileValidationResult
{
    private static FileValidationLocalizer? _localizer;

    private FileValidationResult(bool hasError = false, int? statusCode = null, string? title = null,
        string? detail = null)
    {
        HasError = hasError;
        StatusCode = statusCode;
        Title = title;
        Detail = detail;
    }

    public bool HasError { get; }
    public int? StatusCode { get; }
    public string? Title { get; }
    public string? Detail { get; }

    internal static void Configure(FileValidationLocalizer localizer)
    {
        _localizer = localizer;
    }

    public static FileValidationResult Success()
    {
        return new FileValidationResult();
    }

    public static FileValidationResult FileTooLarge(long maxFileSize)
    {
        return new FileValidationResult(
            true,
            StatusCodes.Status413PayloadTooLarge,
            _localizer?.GetLocalizedString(FileValidationResource.FileTooLarge) ?? FileValidationResource.FileTooLarge,
            _localizer?.GetLocalizedString(FileValidationResource.FileSizeExceedsLimit, maxFileSize / 1024 / 1024) ?? 
            string.Format(FileValidationResource.FileSizeExceedsLimit, maxFileSize / 1024 / 1024));
    }

    public static FileValidationResult UnsupportedExtension(string extension)
    {
        return new FileValidationResult(
            true,
            StatusCodes.Status415UnsupportedMediaType,
            _localizer?.GetLocalizedString(FileValidationResource.UnsupportedFileType) ?? FileValidationResource.UnsupportedFileType,
            _localizer?.GetLocalizedString(FileValidationResource.UnsupportedFileExtension, extension) ?? 
            string.Format(FileValidationResource.UnsupportedFileExtension, extension));
    }

    public static FileValidationResult InvalidFileName(string message)
    {
        return new FileValidationResult(
            true,
            StatusCodes.Status400BadRequest,
            _localizer?.GetLocalizedString(FileValidationResource.InvalidFileName) ?? FileValidationResource.InvalidFileName,
            _localizer?.GetLocalizedString(message) ?? message);
    }

    public IActionResult ToProblemDetails()
    {
        if (!HasError) throw new InvalidOperationException("Cannot create ProblemDetails for successful validation");

        return new ObjectResult(new ProblemDetails
        {
            Status = StatusCode,
            Title = Title,
            Detail = Detail
        })
        {
            StatusCode = StatusCode
        };
    }
} 