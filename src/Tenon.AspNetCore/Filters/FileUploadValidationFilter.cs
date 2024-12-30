using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Tenon.AspNetCore.Configuration;
using Tenon.AspNetCore.Filters.Validators;

namespace Tenon.AspNetCore.Filters;

public class FileUploadValidationFilter : IAsyncActionFilter
{
    private readonly FileUploadOptions _options;
    private readonly IEnumerable<IFileValidator> _validators;

    public FileUploadValidationFilter(IOptions<FileUploadOptions> options)
    {
        _options = options.Value;
        _validators = CreateValidators();
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var files = ExtractFilesFromContext(context);

        foreach (var file in files)
        foreach (var validator in _validators)
        {
            var result = validator.Validate(file);
            if (result.HasError)
            {
                context.Result = result.ToProblemDetails();
                return;
            }
        }

        await next();
    }

    private IEnumerable<IFileValidator> CreateValidators()
    {
        var validators = new List<IFileValidator>
        {
            new FileSizeValidator(_options.MaxFileSize),
            new FileExtensionValidator(_options.AllowedExtensions)
        };

        if (_options.ValidateFileName)
            validators.Add(new FileNameValidator());

        if (_options.AllowedContentTypes?.Count > 0)
            validators.Add(new ContentTypeValidator(_options.AllowedContentTypes));

        return validators;
    }

    private static IEnumerable<IFormFile> ExtractFilesFromContext(ActionExecutingContext context)
    {
        return context.ActionArguments.Values
            .OfType<IFormFile>()
            .Concat(context.ActionArguments.Values
                .OfType<IFormFileCollection>()
                .SelectMany(x => x));
    }
}