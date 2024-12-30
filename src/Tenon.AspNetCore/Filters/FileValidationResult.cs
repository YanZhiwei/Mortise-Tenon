using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tenon.AspNetCore.Filters;

public class FileValidationResult
{
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

    public static FileValidationResult Success()
    {
        return new FileValidationResult();
    }

    public static FileValidationResult FileTooLarge(long maxFileSize)
    {
        return new FileValidationResult(
            true,
            StatusCodes.Status413PayloadTooLarge,
            "File Too Large",
            $"The file size cannot exceed {maxFileSize / 1024 / 1024}MB");
    }

    public static FileValidationResult UnsupportedExtension(string extension)
    {
        return new FileValidationResult(
            true,
            StatusCodes.Status415UnsupportedMediaType,
            "Unsupported File Type",
            $"The file extension '{extension}' is not supported");
    }

    public static FileValidationResult InvalidFileName(string message)
    {
        return new FileValidationResult(
            true,
            StatusCodes.Status400BadRequest,
            "Invalid File Name",
            message);
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