using FluentValidation;

namespace Tenon.FluentValidation.Extensions;

/// <summary>
///     https://docs.fluentvalidation.net/en/latest/aspnet.html
/// </summary>
public static class FluentValidationExtension
{
    public static IRuleBuilderOptions<T, TProperty> Required<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .NotEmpty()
            .WithMessage("{PropertyName} is required");
    }

    public static IRuleBuilderOptions<T, string> FileName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotNull().WithMessage("{PropertyName} cannot be null")
            .NotEmpty().WithMessage("{PropertyName} cannot be empty")
            .Length(1, 255).WithMessage("{PropertyName} must be between 1 and 255 characters long")
            .Matches(@"^[^\\/:*?""<>|]+$").WithMessage("{PropertyName} contains invalid characters");
    }
}